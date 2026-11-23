using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using SomaFrameQD.Framework;
using SomaFrameQD.Framework.Services;
using SomaFrameQD.Modules.ErrorList.Properties;

namespace SomaFrameQD.Modules.ErrorList.ViewModels
{
    [Export(typeof(IErrorList))]
    public class ErrorListViewModel : Tool, IErrorList
    {
        private readonly BindableCollection<ErrorListItem> _items;

        public override PaneLocation PreferredLocation
        {
            get { return PaneLocation.Bottom; }
        }

        public IObservableCollection<ErrorListItem> Items
        {
            get { return _items; }
        }

        public IEnumerable<ErrorListItem> FilteredItems
        {
            get
            {
                if (ShowErrors && ShowWarnings && ShowMessages)
                    return _items;

                if (!ShowErrors && !ShowWarnings && !ShowMessages)
                    return Enumerable.Empty<ErrorListItem>();

                return _items.Where(IsItemVisible);
            }
        }

        private bool _showErrors = true;
        public bool ShowErrors
        {
            get { return _showErrors; }
            set
            {
                if (_showErrors == value)
                    return;
                _showErrors = value;
                NotifyOfPropertyChange(() => ShowErrors);
                NotifyOfPropertyChange("FilteredItems");
            }
        }

        private bool _showWarnings = true;
        public bool ShowWarnings
        {
            get { return _showWarnings; }
            set
            {
                if (_showWarnings == value)
                    return;
                _showWarnings = value;
                NotifyOfPropertyChange(() => ShowWarnings);
                NotifyOfPropertyChange("FilteredItems");
            }
        }

        private bool _showMessages = true;
        public bool ShowMessages
        {
            get { return _showMessages; }
            set
            {
                if (_showMessages == value)
                    return;
                _showMessages = value;
                NotifyOfPropertyChange(() => ShowMessages);
                NotifyOfPropertyChange("FilteredItems");
            }
        }

        public ErrorListViewModel()
        {
            DisplayName = Resources.ViewErrorListCommandToolTip;

            ToolBarDefinition = ToolBarDefinitions.ErrorListToolBar;

            _items = new BindableCollection<ErrorListItem>();
            _items.CollectionChanged += (sender, e) =>
            {
                NotifyOfPropertyChange("FilteredItems");
                NotifyOfPropertyChange("ErrorItemCount");
                NotifyOfPropertyChange("WarningItemCount");
                NotifyOfPropertyChange("MessageItemCount");
            };
        }

        public void AddItem(
            ErrorListItemType itemType,
            string description,
            string path = null,
            int? line = null,
            int? column = null,
            System.Action onClick = null)
        {
            Items.Add(new ErrorListItem(itemType, Items.Count + 1, description, path, line, column)
            {
                OnClick = onClick
            });
        }

        private bool IsItemVisible(ErrorListItem item)
        {
            switch (item.ItemType)
            {
                case ErrorListItemType.Error:
                    return ShowErrors;
                case ErrorListItemType.Warning:
                    return ShowWarnings;
                case ErrorListItemType.Message:
                    return ShowMessages;
                default:
                    return true;
            }
        }
    }
}
