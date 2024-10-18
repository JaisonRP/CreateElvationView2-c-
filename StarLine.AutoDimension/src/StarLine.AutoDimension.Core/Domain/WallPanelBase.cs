using System;

namespace StarLine.AutoDimension.Core.Domain
{
    public abstract class WallPanelBase
    {
        protected WallPanelBase(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public bool IsByPass { get; set; }
        public bool IsHeelLeft { get; set; }
        public bool IsHeelRight { get; set; }


        public string Series { get; set; }

        public string FrameTag { get; set; }


        public bool IsDoor => !string.IsNullOrEmpty(Series) &&
                              Series.StartsWith("D", StringComparison.InvariantCultureIgnoreCase);
    }
}