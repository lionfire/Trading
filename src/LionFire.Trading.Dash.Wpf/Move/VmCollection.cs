
// Retrieved from http://stackoverflow.com/a/15831128/208304

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Avalon
{
    public interface IViewModel { object Model { get; set; } }
    public interface IViewModel<T> : IViewModel
    {
        new T Model { get; set; }
        bool IsViewModelOf(object obj);
    }
    public interface IViewModelProvider
    {
        T GetFor<T>(object model, object context);
    }

    //public class ViewModel<T> : IViewModel<T>
    //{
    //    object IViewModel.Model { get { return Model; } set { Model = (T)value; } }
    //    public T Model { get; set; }
    //    bool IsViewModelOf(object obj);
    //}

    public class ViewModelProvider : IViewModelProvider
    {
        public T GetFor<T>(object model, object context)
        {
            return default(T);
        }
    }

    //public interface IViewModelProvider<T> : IViewModelProvider
    //    where T : IViewModel
    //{

    //}

    /// <summary>
    /// Observable collection of ViewModels that pushes changes to a related collection of models
    /// </summary>
    /// <typeparam name="TViewModel">Type of ViewModels in collection</typeparam>
    /// <typeparam name="TModel">Type of models in underlying collection</typeparam>
    public class VmCollection<TViewModel, TModel> : ObservableCollection<TViewModel>
        where TViewModel : class, IViewModel
        where TModel : class

    {
        private readonly object _context;
        private readonly ICollection<TModel> _models;
        private bool _synchDisabled;
        private readonly IViewModelProvider _viewModelProvider;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="models">List of models to synch with</param>
        /// <param name="viewModelProvider"></param>
        /// <param name="context"></param>
        /// <param name="autoFetch">
        /// Determines whether the collection of ViewModels should be
        /// fetched from the model collection on construction
        /// </param>
        public VmCollection(ICollection<TModel> models, IViewModelProvider viewModelProvider = null, object context = null, bool autoFetch = true)
        {
            _models = models;
            _context = context;

            _viewModelProvider = viewModelProvider;

            // Register change handling for synchronization
            // from ViewModels to Models
            CollectionChanged += ViewModelCollectionChanged;

            // If model collection is observable register change
            // handling for synchronization from Models to ViewModels
            if (models is ObservableCollection<TModel>)
            {
                var observableModels = models as ObservableCollection<TModel>;
                observableModels.CollectionChanged += ModelCollectionChanged;
            }


            // Fecth ViewModels
            if (autoFetch) FetchFromModels();
        }

        /// <summary>
        /// CollectionChanged event of the ViewModelCollection
        /// </summary>
        public override sealed event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { base.CollectionChanged += value; }
            remove { base.CollectionChanged -= value; }
        }

        /// <summary>
        /// Load VM collection from model collection
        /// </summary>
        public void FetchFromModels()
        {
            // Deactivate change pushing
            _synchDisabled = true;

            // Clear collection
            Clear();

            // Create and add new VM for each model
            foreach (var model in _models)
                AddForModel(model);

            // Reactivate change pushing
            _synchDisabled = false;
        }

        private void ViewModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Return if synchronization is internally disabled
            if (_synchDisabled) return;

            // Disable synchronization
            _synchDisabled = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var m in e.NewItems.OfType<IViewModel>().Select(v => v.Model).OfType<TModel>())
                        _models.Add(m);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var m in e.OldItems.OfType<IViewModel>().Select(v => v.Model).OfType<TModel>())
                        _models.Remove(m);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    _models.Clear();
                    foreach (var m in e.NewItems.OfType<IViewModel>().Select(v => v.Model).OfType<TModel>())
                        _models.Add(m);
                    break;
            }

            //Enable synchronization
            _synchDisabled = false;
        }

        private void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_synchDisabled) return;
            _synchDisabled = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var m in e.NewItems.OfType<TModel>())
                        this.AddIfNotNull(CreateViewModel(m));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var m in e.OldItems.OfType<TModel>())
                        this.Remove(GetViewModelOfModel(m));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    FetchFromModels();
                    break;
            }

            _synchDisabled = false;
        }

        private void AddIfNotNull(TViewModel viewModel)
        {
            if (viewModel != null)
            {
                this.Add(viewModel);
            }
        }

        private TViewModel CreateViewModel(TModel model)
        {
            if (_viewModelProvider == null)
            {
                throw new Exception("No ViewModelProvider was provided at create time.  Cannot CreateViewModel.");
            }
            return _viewModelProvider.GetFor<TViewModel>(model, _context);
        }

        private TViewModel GetViewModelOfModel(TModel model)
        {
            return Items.OfType<IViewModel<TModel>>().FirstOrDefault(v => v.IsViewModelOf(model)) as TViewModel;
        }

        /// <summary>
        /// Adds a new ViewModel for the specified Model instance
        /// </summary>
        /// <param name="model">Model to create ViewModel for</param>
        public void AddForModel(TModel model)
        {
            Add(CreateViewModel(model));
        }

        /// <summary>
        /// Adds a new ViewModel with a new model instance of the specified type,
        /// which is the ModelType or derived from the Model type
        /// </summary>
        /// <typeparam name="TSpecificModel">Type of Model to add ViewModel for</typeparam>
        public void AddNew<TSpecificModel>() where TSpecificModel : TModel, new()
        {
            var m = new TSpecificModel();
            Add(CreateViewModel(m));
        }
    }
}
