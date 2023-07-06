using System.Linq;
using System.Windows.Input;
using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Core.Utils;
using Tortuga.Anchor.Collections;
using Tortuga.Anchor.Modeling;

namespace StarLine.AutoDimension.Core.ViewModels
{
    public class ReferenceViewModel : ModelBase
    {
        public ObservableCollectionExtended<ReferenceModel> References { get; } = new ObservableCollectionExtended<ReferenceModel>();

        public ReferenceModel SelectedReference
        {
            get => Get<ReferenceModel>();
            set => Set(value);
        }

        private ICommand _addCommand;
        public ICommand AddCommand => _addCommand ??
                                     (_addCommand = new RelayCommand(p => true, Add));

        public void Add(object p)
        {
            var nextIndex = References.Max(x => x.ExtractIndex()) + 1;
            References.Add(new ReferenceModel
            {
                RefType = RefType.ClBar,
                TypeName = $"CL TBar {nextIndex}",
                PlaneName = $"CL_TBar_{nextIndex}"
            });
        }

        private ICommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ??
                                     (_deleteCommand = new RelayCommand(CanDelete, Delete));

        private bool CanDelete(object obj)
        {
            return SelectedReference != null && SelectedReference.RefType == RefType.ClBar;
        }

        public void Delete(object p)
        {
            References.Remove(SelectedReference);
        }
    }
}
