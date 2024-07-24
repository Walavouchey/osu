// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Menu
{
    public partial class SongTicker : Container
    {
        private const int fade_duration = 800;

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; } = null!;

        private readonly OsuSpriteText title, artist;

        public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

        public SongTicker()
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        private void show()
        {
        }
    }
}
