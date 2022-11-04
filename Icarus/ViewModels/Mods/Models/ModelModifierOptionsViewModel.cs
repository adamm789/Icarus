using Icarus.ViewModels.Util;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Models.Helpers;

namespace Icarus.ViewModels.Import
{
    public class ModelModifierOptionsViewModel : NotifyPropertyChanged
    {
        // TODO: Find a beter place for these
        public static readonly string UseOriginalShapeDataName = "Use Original Shape Data";
        public static readonly string UseOriginalShapeDataTooltip = "TODO";

        public static readonly string ForceUVQuadrantName = "Force UV to [1,-1]";
        public static readonly string ForceUVQuadrantTooltip = "TODO";

        public static readonly string ClearUV2Name = "Clear UV2";
        public static readonly string ClearUV2Tooltip = "TODO";

        public static readonly string CloneUV2Name = "Clone UV2";
        public static readonly string CloneUV2Tooltip = "TODO";

        public static readonly string ClearVColorName = "Clear Vertex Color";
        public static readonly string ClearVColorTooltip = "TODO";

        public static readonly string ClearVAlphaName = "Clear Vertex Alpha";
        public static readonly string ClearVAlphaTooltip = "TODO";

        public static readonly string AutoScaleName = "Auto-Scale";

        // I believe that this is the case
        public static readonly string OverrideRaceToolTip = "The race the imported model is scaled to.";

        public bool UseOriginalShapeData
        {
            get { return _options.UseOriginalShapeData; }
            set { _options.UseOriginalShapeData = value; OnPropertyChanged(); }
        }

        public bool ForceUVQuadrant
        {
            get { return _options.ForceUVQuadrant; }
            set { _options.ForceUVQuadrant = value; OnPropertyChanged(); }
        }

        public bool ClearUV2
        {
            get { return _options.ClearUV2; }
            set { _options.ClearUV2 = value; OnPropertyChanged(); }
        }

        public bool CloneUV2
        {
            get { return _options.CloneUV2; }
            set { _options.CloneUV2 = value; OnPropertyChanged(); }
        }

        public bool ClearVColor
        {
            get { return _options.ClearVColor; }
            set { _options.ClearVColor = value; OnPropertyChanged(); }
        }

        public bool ClearVAlpha
        {
            get { return _options.ClearVAlpha; }
            set { _options.ClearVAlpha = value; OnPropertyChanged(); }
        }
        public bool AutoScale
        {
            get { return _options.AutoScale; }
            set { _options.AutoScale = value; OnPropertyChanged(); }
        }

        // Changes based on the parent model's TargetRace
        // Only assigned to SourceRace if OverrideRace is true
        XivRace _sourceRace = XivRace.Hyur_Midlander_Male;

        // TODO: Do I want to SourceRace (when OverrideRace is true)
        // to change to _sourceRace? Or stay the same?
        public XivRace SourceRace
        {
            get { return _options.SourceRace; }
            set
            {
                _options.SourceRace = value;
                OnPropertyChanged();
                if (value != XivRace.All_Races)
                {
                    _sourceRace = value;
                }
            }
        }

        bool _overrideRace = false;
        public bool OverrideRace
        {
            get { return _overrideRace; }
            set
            {
                _overrideRace = value;
                OnPropertyChanged();
                if (_overrideRace)
                {
                    SourceRace = _sourceRace;
                }
                else
                {
                    SourceRace = XivRace.All_Races;
                }
            }
        }

        ModelModifierOptions _options;
        public ModelModifierOptionsViewModel(ModelModifierOptions options)
        {
            _options = options;

            // Probably?
            AutoScale = false;
            ForceUVQuadrant = true;
        }

        public void UpdateTargetRace(XivRace race)
        {
            //_sourceRace = race;
            if (OverrideRace)
            {
                //SourceRace = race;

                SourceRace = _sourceRace;
            }
            else
            {
                _sourceRace = race;
                //SourceRace = XivRace.All_Races;
            }
        }
    }
}
