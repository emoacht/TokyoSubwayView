using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TokyoSubwayView.Views.Behaviors
{
	public class ListViewSelectBehavior : DependencyObject, IBehavior
	{
		public DependencyObject AssociatedObject { get; private set; }

		private ListView AssociatedListView
		{
			get { return (ListView)this.AssociatedObject; }
		}

		public void Attach(DependencyObject associatedObject)
		{
			this.AssociatedObject = associatedObject as ListView;

			if (this.AssociatedObject == null)
			{
				Debug.WriteLine("Associated object is not ListView!");
				return;
			}

			AssociatedListView.SelectionChanged += OnSelectionChanged;
		}

		public void Detach()
		{
			if (this.AssociatedObject == null)
				return;

			AssociatedListView.SelectionChanged -= OnSelectionChanged;
		}


		public int[] SelectedIndices
		{
			get { return (int[])GetValue(SelectedIndicesProperty); }
			set { SetValue(SelectedIndicesProperty, value); }
		}
		public static readonly DependencyProperty SelectedIndicesProperty =
			DependencyProperty.Register(
				"SelectedIndices",
				typeof(int[]),
				typeof(ListViewSelectBehavior),
				new PropertyMetadata(
					null,
					(d, e) => ((ListViewSelectBehavior)d).OnSelectedIndicesChanged(e)));


		private bool _isChanging;

		private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_isChanging)
				return;

			try
			{
				_isChanging = true;

				var items = AssociatedListView.Items;

				SelectedIndices = AssociatedListView.SelectedItems
					.Select(x => items.IndexOf(x))
					.ToArray();
			}
			finally
			{
				_isChanging = false;
			}
		}

		private void OnSelectedIndicesChanged(DependencyPropertyChangedEventArgs e)
		{
			if (_isChanging)
				return;

			try
			{
				_isChanging = true;

				if (SelectedIndices == null)
				{
					AssociatedListView.SelectedItems.Clear();
					return;
				}

				var items = AssociatedListView.Items;

				foreach (var item in AssociatedListView.SelectedItems)
				{
					if (!SelectedIndices.Contains(items.IndexOf(item)))
						AssociatedListView.SelectedItems.Remove(item);
				}

				foreach (var index in SelectedIndices)
				{
					if (!AssociatedListView.SelectedItems.Contains(items[index]))
						AssociatedListView.SelectedItems.Add(items[index]);
				}
			}
			finally
			{
				_isChanging = false;
			}
		}
	}
}