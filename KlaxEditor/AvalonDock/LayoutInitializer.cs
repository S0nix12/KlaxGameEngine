﻿using Xceed.Wpf.AvalonDock.Layout;

namespace KlaxEditor
{
    class CLayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }


        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }
    }
}
