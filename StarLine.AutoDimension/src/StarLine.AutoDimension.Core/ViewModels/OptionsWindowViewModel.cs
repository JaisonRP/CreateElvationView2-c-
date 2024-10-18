using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Core.Services;
using StarLine.AutoDimension.Core.Utils;
using StarLine.AutoDimension.Core.Views;
using Tortuga.Anchor;
using Tortuga.Anchor.DataAnnotations;
using Tortuga.Anchor.Modeling;

namespace StarLine.AutoDimension.Core.ViewModels
{
    public class OptionsWindowViewModel : ModelBase
    {
        public double ScaleFactor { get; private set; }

        public ObservableCollection<IdNamePair> PanelTags { get; } = new ObservableCollection<IdNamePair>();

        public ObservableCollection<IdNamePair> DoorTags { get; } = new ObservableCollection<IdNamePair>();

        public ObservableCollection<IdNamePair> MaterialTags { get; } = new ObservableCollection<IdNamePair>();

        public ObservableCollection<IdNamePair> GenericTags { get; } = new ObservableCollection<IdNamePair>();

        public ObservableCollection<IdNamePair> AnnotationSymbols { get; } = new ObservableCollection<IdNamePair>();

        public ObservableCollection<IdNamePair> DimensionStyles { get; } = new ObservableCollection<IdNamePair>();

        public ReferenceViewModel VerticalReferences
        {
            get => GetNew<ReferenceViewModel>();
            set => Set(value);
        }

        public ReferenceViewModel HorizontalReferences
        {
            get => GetNew<ReferenceViewModel>();
            set => Set(value);
        }

        public string UnitName
        {
            get => Get<string>();
            set => Set(value);
        }

        public IdNamePair CurtainPanelTag
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair CurtainPanelTagDoors
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair MaterialTag
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair GenericTag
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair GenericTag2
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair GenericAnnotation
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public IdNamePair DimensionStyle
        {
            get => Get<IdNamePair>();
            set => Set(value);
        }

        public bool SuppressCornerPostTag
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SuppressAnnotation
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SuppressGenericTag
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool HorizontalDimensionBottom
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool VerticalDimensionLeft
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SupressVerticalDimension
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool SupressHorizontalDimension
        {
            get => Get<bool>();
            set => Set(value);
        }
        public bool ReplaceWithText
        {
            get => Get<bool>();
            set => Set(value);
        }
        

        public bool AutoAlignDimension
        {
            get => Get<bool>();
            set => Set(value);
        }

        public double CurtainTagOffset
        {
            get => Get<double>();
            set => Set(value);
        }

        public double DoorTagOffset
        {
            get => Get<double>();
            set => Set(value);
        }

        public double MaterialTagOffset
        {
            get => Get<double>();
            set => Set(value);
        }

        public double CornerPostTagOffset
        {
            get => Get<double>();
            set => Set(value);
        }

        public double MoveSegmentsLessThan
        {
            get => Get<double>();
            set => Set(value);
        }

        public double MoveSegmentsBy
        {
            get => Get<double>();
            set => Set(value);
        }

        public double StackSegmentsCloserThan
        {
            get => Get<double>();
            set => Set(value);
        }

        public double StackingDistance
        {
            get => Get<double>();
            set => Set(value);
        }

        public double DimLinesDistance
        {
            get => Get<double>();
            set => Set(value);
        }

        public double FirstDimLineHorizontal
        {
            get => Get<double>();
            set => Set(value);
        }

        public double FirstDimLineVertical
        {
            get => Get<double>();
            set => Set(value);
        }

        private ICommand _importCommand;
        public ICommand ImportCommand => _importCommand ??
                                     (_importCommand = new RelayCommand(p => true, Import));

        public void Import(object p)
        {
            var dlg = new CommonOpenFileDialog
            {
                Title = "Open file",
                DefaultExtension = ".json"
            };

            dlg.Filters.Add(new CommonFileDialogFilter("JSON File", "*.json"));
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var options = OptionSerializer.Deserialize(dlg.FileName);
                ReadFromOptions(options);
            }
        }

        private ICommand _exportCommand;
        public ICommand ExportCommand => _exportCommand ??
                                     (_exportCommand = new RelayCommand(p => true, Export));

        public void Export(object p)
        {
            var dlg = new CommonSaveFileDialog
            {
                Title = "Save file",
                DefaultExtension = ".json"
            };

            dlg.Filters.Add(new CommonFileDialogFilter("JSON File", "*.json"));
            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var options = ExportOptions();
                OptionSerializer.Serialize(options, dlg.FileName);
            }
        }

        private ICommand _resetCommand;
        public ICommand ResetCommand => _resetCommand ??
                                     (_resetCommand = new RelayCommand(p => true, Reset));

        public void Reset(object p)
        {
            var options = OptionSerializer.ResetToDefault();
            ReadFromOptions(options);
        }

        private ICommand _saveCommand;
        public ICommand SaveCommand => _saveCommand ??
                                     (_saveCommand = new RelayCommand(p => true, Save));

        public void Save(object p)
        {
            if (HasErrors)
            {
                return;
            }

            OptionSerializer.WriteCurrent(ExportOptions());
        }

        public void ReadFromOptions(Options options)
        {
            CurtainPanelTag = IdNamePair.GetByIdOrName(PanelTags, options.CurtainPanelTag);
            CurtainPanelTagDoors = IdNamePair.GetByIdOrName(DoorTags, options.CurtainPanelTagDoors);
            MaterialTag = IdNamePair.GetByIdOrName(MaterialTags, options.MaterialTag);
            GenericTag = IdNamePair.GetByIdOrName(GenericTags, options.GenericTag);
            GenericTag2 = IdNamePair.GetByIdOrName(GenericTags, options.GenericTag2);
            GenericAnnotation = IdNamePair.GetByIdOrName(AnnotationSymbols, options.GenericAnnotation);
            DimensionStyle = IdNamePair.GetByIdOrName(DimensionStyles, options.DimensionStyle);
            SuppressCornerPostTag = options.SuppressCornerPostTag;
            SuppressAnnotation = options.SuppressAnnotation;
            SuppressGenericTag = options.SuppressGenericTag;
            HorizontalDimensionBottom = options.HorizontalDimensionBottom;
            VerticalDimensionLeft = options.VerticalDimensionLeft;
            SupressVerticalDimension = options.SupressVerticalDimension; // new option
            SupressHorizontalDimension = options.SupressHorizontalDimension; // new option v 1.5.0.6
            SupressHorizontalDimension = options.ReplaceWithText; // new option v 1.5.0.7
            AutoAlignDimension = options.AutoAlignDimension;
            CurtainTagOffset = options.CurtainTagOffset * ScaleFactor;
            DoorTagOffset = options.DoorTagOffset * ScaleFactor;
            MaterialTagOffset = options.MaterialTagOffset * ScaleFactor;
            CornerPostTagOffset = options.CornerPostTagOffset * ScaleFactor;
            MoveSegmentsLessThan = options.MoveSegmentsLessThan * ScaleFactor;
            MoveSegmentsBy = options.MoveSegmentsBy * ScaleFactor;
            StackSegmentsCloserThan = options.StackSegmentsCloserThan * ScaleFactor;
            StackingDistance = options.StackingDistance * ScaleFactor;
            DimLinesDistance = options.DimLinesDistance * ScaleFactor;
            FirstDimLineHorizontal = options.FirstDimLineHorizontal * ScaleFactor;
            FirstDimLineVertical = options.FirstDimLineVertical * ScaleFactor;
            VerticalReferences.References.Clear();
            VerticalReferences.References.AddRange(options.VerticalReferences.Select(ConvertToReferenceModel));
            HorizontalReferences.References.Clear();
            HorizontalReferences.References.AddRange(options.HorizontalReferences.Select(ConvertToReferenceModel));
        }

        private Options ExportOptions()
        {
            var options = new Options
            {
                CurtainPanelTag = CurtainPanelTag,
                CurtainPanelTagDoors = CurtainPanelTagDoors,
                MaterialTag = MaterialTag,
                GenericTag = GenericTag,
                GenericTag2 = GenericTag2,
                GenericAnnotation = GenericAnnotation,
                DimensionStyle = DimensionStyle,
                SuppressCornerPostTag = SuppressCornerPostTag,
                SuppressAnnotation = SuppressAnnotation,
                SuppressGenericTag = SuppressGenericTag,
                HorizontalDimensionBottom = HorizontalDimensionBottom,
                VerticalDimensionLeft = VerticalDimensionLeft,
                SupressVerticalDimension = SupressVerticalDimension, // new option
                SupressHorizontalDimension = SupressHorizontalDimension, // new option v 1.5.0.6
                ReplaceWithText = ReplaceWithText, // new option v 1.5.0.7
                AutoAlignDimension = AutoAlignDimension,
                CurtainTagOffset = CurtainTagOffset / ScaleFactor,
                DoorTagOffset = DoorTagOffset / ScaleFactor,
                MaterialTagOffset = MaterialTagOffset / ScaleFactor,
                CornerPostTagOffset = CornerPostTagOffset / ScaleFactor,
                MoveSegmentsLessThan = MoveSegmentsLessThan / ScaleFactor,
                MoveSegmentsBy = MoveSegmentsBy / ScaleFactor,
                StackSegmentsCloserThan = StackSegmentsCloserThan / ScaleFactor,
                StackingDistance = StackingDistance / ScaleFactor,
                DimLinesDistance = DimLinesDistance / ScaleFactor,
                FirstDimLineHorizontal = FirstDimLineHorizontal / ScaleFactor,
                FirstDimLineVertical = FirstDimLineVertical / ScaleFactor
            };

            options.VerticalReferences.AddRange(VerticalReferences.References.Select(ConvertToReferenceRepresentation));
            options.HorizontalReferences.AddRange(HorizontalReferences.References.Select(ConvertToReferenceRepresentation));
            return options;
        }

        private static ReferenceModel ConvertToReferenceModel(ReferenceRepresentation referenceRepresentation)
        {
            return new ReferenceModel
            {
                PlaneName = referenceRepresentation.PlaneName,
                RefType = referenceRepresentation.RefType,
                TypeName = referenceRepresentation.TypeName
            };
        }

        private static ReferenceRepresentation ConvertToReferenceRepresentation(ReferenceModel referenceModel)
        {
            return new ReferenceRepresentation(referenceModel.RefType)
            {
                PlaneName = referenceModel.PlaneName,
                TypeName = referenceModel.TypeName
            };
        }

        public void SetLengthSymbol(string lengthSymbol, double factor)
        {
            UnitName = lengthSymbol;
            ScaleFactor = factor;
        }

        public void SetPanelTags(IEnumerable<IdNamePair> panelSymbols)
        {
            var panelTag = CurtainPanelTag;
            PanelTags.Clear();
            PanelTags.AddRange(panelSymbols);
            CurtainPanelTag = IdNamePair.GetByIdOrName(PanelTags, panelTag);
        }

        public void SetDoorTags(IEnumerable<IdNamePair> doorSymbols)
        {
            var panelDoorTag = CurtainPanelTagDoors;
            DoorTags.Clear();
            DoorTags.AddRange(doorSymbols);
            CurtainPanelTagDoors = IdNamePair.GetByIdOrName(DoorTags, panelDoorTag);
        }

        public void SetMaterialTags(IEnumerable<IdNamePair> symbols)
        {
            var currentTag = MaterialTag;
            MaterialTags.Clear();
            MaterialTags.AddRange(symbols);
            MaterialTag = IdNamePair.GetByIdOrName(MaterialTags, currentTag);
        }

        public void SetGenericTags(IEnumerable<IdNamePair> symbols)
        {
            var currentTag = GenericTag;
            GenericTags.Clear();
            GenericTags.AddRange(symbols);
            GenericTag = IdNamePair.GetByIdOrName(GenericTags, currentTag);
        }

        public void SetAnnotationSymbols(IEnumerable<IdNamePair> symbols)
        {
            var genericAnnotation = GenericAnnotation;
            AnnotationSymbols.Clear();
            AnnotationSymbols.AddRange(symbols);
            GenericAnnotation = IdNamePair.GetByIdOrName(AnnotationSymbols, genericAnnotation);
        }

        public void SetDimensionStyles(IEnumerable<IdNamePair> symbols)
        {
            var dimensionStyle = DimensionStyle;
            DimensionStyles.Clear();
            DimensionStyles.AddRange(symbols);
            DimensionStyle = IdNamePair.GetByIdOrName(AnnotationSymbols, dimensionStyle);
        }

        protected override void OnValidateProperty(string propertyName, ValidationResultCollection results)
        {
            switch (propertyName)
            {
                case nameof(MoveSegmentsLessThan):
                    if (MoveSegmentsLessThan < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(MoveSegmentsLessThan));
                    }

                    break;
                case nameof(MoveSegmentsBy):
                    if (MoveSegmentsBy < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(MoveSegmentsBy));
                    }

                    break;
                case nameof(StackSegmentsCloserThan):
                    if (StackSegmentsCloserThan < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(StackSegmentsCloserThan));
                    }

                    break;
                case nameof(StackingDistance):
                    if (StackingDistance < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(StackingDistance));
                    }

                    break;
                case nameof(DimLinesDistance):
                    if (DimLinesDistance < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(DimLinesDistance));
                    }

                    break;
                case nameof(FirstDimLineHorizontal):
                    if (FirstDimLineHorizontal < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(FirstDimLineHorizontal));
                    }

                    break;
                case nameof(FirstDimLineVertical):
                    if (FirstDimLineVertical < 0)
                    {
                        results.Add("Value must be positive or zero", nameof(FirstDimLineVertical));
                    }

                    break;
            }
        }
    }
}
