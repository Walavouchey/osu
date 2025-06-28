// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Menu
{
    public partial class IntroContest : IntroScreen
    {
        protected override string BeatmapHash => "a1556d0801b3a6b175dda32ef546f0ec812b400499f575c44fccbe9c67f9b1e5";

        protected override string BeatmapFile => "triangles.osz";

        protected const double normal_intro_delay = 3200; // 00:03:839 - 00:04:613 -

        [Resolved]
        private AudioManager audio { get; set; }

        private Sample welcome;

        private ContestIntroSequence intro;

        public IntroContest([CanBeNull] Func<MainMenu> createNextScreen = null)
            : base(createNextScreen)
        { }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"Intro/welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                PrepareMenuLoad();

                var decouplingClock = new DecouplingFramedClock(UsingThemedIntro ? Track : null);

                LoadComponentAsync(intro = new ContestIntroSequence(logo, () => FadeInBackground())
                {
                    RelativeSizeAxes = Axes.Both,
                    Clock = new InterpolatingFramedClock(decouplingClock),
                    LoadMenu = LoadMenu
                }, _ =>
                {
                    AddInternal(intro);

                    // There is a chance that the intro timed out before being displayed, and this scheduled callback could
                    // happen during the outro rather than intro.
                    // In such a scenario, we don't want to play the intro sample, nor attempt to start the intro track
                    // (that may have already been since disposed by MusicController).
                    if (DidLoadMenu)
                        return;

                    if (!UsingThemedIntro)
                    {
                        // If the user has requested no theme, fallback to the same intro voice and delay as IntroCircles.
                        // The triangles intro voice and theme are combined which makes it impossible to use.
                        //Scheduler.AddDelayed(() => welcome?.Play(), normal_intro_delay);
                        //Scheduler.AddDelayed(StartTrack, IntroCircles.TRACK_START_DELAY);
                        StartTrack();
                    }
                    else
                        StartTrack();

                    // no-op for the case of themed intro, no harm in calling for both scenarios as a safety measure.
                    decouplingClock.Start();
                });
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);

            // important as there is a clock attached to a track which will likely be disposed before returning to this screen.
            intro.Expire();
        }

        private partial class ContestIntroSequence : CompositeDrawable
        {
            private readonly OsuLogo logo;
            private readonly Action showBackgroundAction;
            private OsuSpriteText welcomeText;

            private RulesetFlow rulesets;
            private Container rulesetsScale;
            private Container logoContainerSecondary;
            private LazerLogo lazerLogo;
            private Artwork artwork;
            private Container artworkScale;
            private Box dimmer;

            public Action LoadMenu;

            public ContestIntroSequence(OsuLogo logo, Action showBackgroundAction)
            {
                this.logo = logo;
                this.showBackgroundAction = showBackgroundAction;
            }

            [Resolved]
            private OsuGameBase game { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    artworkScale = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = artwork = new Artwork
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    },
                    dimmer = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both,
                    },
                    welcomeText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Padding = new MarginPadding { Bottom = 10 },
                        Font = OsuFont.GetFont(weight: FontWeight.Light, size: 42),
                        Alpha = 1,
                        Spacing = new Vector2(5),
                    },
                    rulesetsScale = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            rulesets = new RulesetFlow()
                        }
                    },
                    logoContainerSecondary = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Child = lazerLogo = new LazerLogo
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    },
                };
            }

            private const double lead_in = 1100;

            private const double text_1 = 200;
            private const double text_2 = 400;
            private const double text_3 = 700;
            private const double text_4 = 900;
            private const double text_glitch = 1060;

            private const double rulesets_1 = 1450;
            private const double rulesets_2 = 1650;
            private const double rulesets_3 = 1850;

            private const double logo_scale_duration = 920;
            private const double logo_1 = 2080;
            private const double logo_2 = logo_1 + logo_scale_duration;

            protected override void LoadComplete()
            {
                base.LoadComplete();

                const float scale_start = 1.2f;
                const float scale_adjust = 0.8f;

                rulesets.Hide();
                lazerLogo.Hide();
                artwork.trail.Hide();
                artwork.fairy.Hide();
                artwork.pippi.Hide();
                artwork.wand.Hide();

                using (BeginAbsoluteSequence(0))
                {
                    artworkScale.ScaleTo(1.4f);
                    const double beat_duration = 387;
                    using (BeginDelayedSequence(lead_in))
                    {
                        dimmer.FadeTo(1).Then().FadeTo(0, 1000, Easing.None);
                        artwork.MoveTo(new Vector2(580, 0));
                        artwork.MoveTo(new Vector2(500, 0), beat_duration * 4, Easing.None);
                        artwork.clouds.MoveTo(new Vector2(-10, 0), beat_duration * 4, Easing.None);
                        artwork.trees.MoveTo(new Vector2(-50, 0), beat_duration * 4, Easing.None).Then().MoveTo(Vector2.Zero);
                    }

                    using (BeginDelayedSequence(beat_duration * 4 + lead_in))
                    {
                        artwork.MoveTo(new Vector2(-580, 0));
                        artwork.MoveTo(new Vector2(-500, 0), beat_duration * 4, Easing.None);
                        artwork.clouds.MoveTo(new Vector2(-5, 0)).Then().MoveTo(Vector2.Zero, beat_duration * 4, Easing.None);
                        artwork.dragon.MoveTo(new Vector2(-15, 0)).Then().MoveTo(Vector2.Zero, beat_duration * 4, Easing.None);
                        artwork.town.MoveTo(new Vector2(-20, 0)).Then().MoveTo(Vector2.Zero, beat_duration * 4, Easing.None);
                    }

                    using (BeginDelayedSequence(beat_duration * 8 + lead_in))
                    {
                        const double zoom_duration = 3000;
                        const Easing zoom_easing = Easing.OutQuint;
                        artwork.trail.FadeIn();
                        artwork.fairy.FadeIn();
                        artwork.pippi.FadeIn();
                        artwork.wand.FadeIn();
                        artwork.MoveTo(new Vector2(0, 0));

                        artworkScale.ScaleTo(1.1f);
                        artworkScale.ScaleTo(0.7f, zoom_duration, zoom_easing);
                        artwork.clouds.ScaleTo(1.02f).Then().ScaleTo(1.01f, zoom_duration, zoom_easing);
                        artwork.town.ScaleTo(1.05f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.trees.ScaleTo(1.06f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.dragon.ScaleTo(1.063f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.pippi.ScaleTo(1.2f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.wand.ScaleTo(1.2f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.fairy.ScaleTo(1.15f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);

                        artwork.clouds.MoveTo(new Vector2(-15, 0), zoom_duration, Easing.None);
                        artwork.dragon.MoveTo(new Vector2(-10, 0), zoom_duration, Easing.None);

                        const Easing rotation_easing = Easing.OutQuad;
                        artwork.fairy.MoveTo(new Vector2(0, 30)).Then().MoveTo(Vector2.Zero, zoom_duration + 500, Easing.OutElastic);

                        artwork.pippi.RotateTo(-5).Then().RotateTo(0, zoom_duration, rotation_easing);
                        artwork.pippi.MoveTo(new Vector2(0, 30)).Then().MoveTo(Vector2.Zero, zoom_duration, rotation_easing);

                        artwork.trail.ScaleTo(1.2f).Then().ScaleTo(1.0f, zoom_duration, zoom_easing);
                        artwork.trail.MoveTo(new Vector2(0, -15)).Then().MoveTo(Vector2.Zero, zoom_duration, rotation_easing);

                        artwork.wand.RotateTo(7).Then().RotateTo(0, zoom_duration, rotation_easing);
                        artwork.wand.MoveTo(new Vector2(0, 35)).Then().MoveTo(Vector2.Zero, zoom_duration, rotation_easing);
                    }

                    using (BeginDelayedSequence(text_1 + normal_intro_delay + lead_in))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "wel");

                    using (BeginDelayedSequence(text_2 + normal_intro_delay + lead_in))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome");

                    using (BeginDelayedSequence(text_3 + normal_intro_delay + lead_in))
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to");

                    using (BeginDelayedSequence(text_4 + normal_intro_delay + lead_in))
                    {
                        welcomeText.FadeIn().OnComplete(t => t.Text = "welcome to osu!");
                        welcomeText.TransformTo(nameof(welcomeText.Spacing), new Vector2(50, 0), 5000);
                    }

                    using (BeginDelayedSequence(rulesets_1 + normal_intro_delay + lead_in))
                    {
                        lazerLogo.FadeOut().OnComplete(_ =>
                        {
                            using (BeginAbsoluteSequence(33))
                            {
                                artwork.FadeOut().Expire();
                                welcomeText.FadeOut().Expire();
                            }

                            logoContainerSecondary.Remove(lazerLogo, true);

                            game.Add(new GameWideFlash());

                            //logo.FadeIn();

                            showBackgroundAction();

                            LoadMenu();
                        });
                    }
                }
            }

            private partial class GameWideFlash : Box
            {
                private const double flash_length = 1000;

                public GameWideFlash()
                {
                    Colour = Color4.White;
                    RelativeSizeAxes = Axes.Both;
                    Blending = BlendingParameters.Additive;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    this.FadeOutFromOne(flash_length, Easing.Out);
                }
            }

            private partial class Artwork : CompositeDrawable
            {
                public Sprite sky, clouds, trees, town, dragon, trail, fairy, pippi, wand;

                public Artwork()
                {
                    Masking = true;
                    Size = new Vector2(2200, 560);
                }

                [BackgroundDependencyLoader]
                private void load(LargeTextureStore textures)
                {
                    InternalChildren = new Drawable[]
                    {
                        sky = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/sky"),
                        },
                        clouds = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/clouds"),
                        },
                        trees = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/trees"),
                        },
                        town = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/town"),
                        },
                        dragon = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/dragon"),
                        },
                        trail = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/trail"),
                        },
                        fairy = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/fairy"),
                        },
                        pippi = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/pippi"),
                        },
                        wand = new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Contest/wand"),
                        },
                    };
                }
            }

            private partial class LazerLogo : CompositeDrawable
            {
                private LogoAnimation highlight, background;

                public float Progress
                {
                    get => background.AnimationProgress;
                    set
                    {
                        background.AnimationProgress = value;
                        highlight.AnimationProgress = value;
                    }
                }

                public LazerLogo()
                {
                    Size = new Vector2(960);
                }

                [BackgroundDependencyLoader]
                private void load(LargeTextureStore textures)
                {
                    InternalChildren = new Drawable[]
                    {
                        highlight = new LogoAnimation
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Triangles/logo-highlight"),
                            Colour = Color4.White,
                        },
                        background = new LogoAnimation
                        {
                            RelativeSizeAxes = Axes.Both,
                            Texture = textures.Get(@"Intro/Triangles/logo-background"),
                            Colour = OsuColour.Gray(0.6f),
                        },
                    };
                }
            }

            private partial class RulesetFlow : FillFlowContainer
            {
                [BackgroundDependencyLoader]
                private void load(RulesetStore rulesets)
                {
                    AutoSizeAxes = Axes.Both;

                    Anchor = Anchor.Centre;
                    Origin = Anchor.Centre;

                    foreach (var ruleset in rulesets.AvailableRulesets)
                    {
                        try
                        {
                            var icon = new ConstrainedIconContainer
                            {
                                Icon = ruleset.CreateInstance().CreateIcon(),
                                Size = new Vector2(30),
                            };

                            Add(icon);
                        }
                        catch
                        {
                            Logger.Log($"Could not create ruleset icon for {ruleset.Name}. Please check for an update from the developer.", level: LogLevel.Error);
                        }
                    }
                }
            }
        }
    }
}
