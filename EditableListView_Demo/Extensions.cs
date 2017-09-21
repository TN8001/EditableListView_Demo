using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EditableListView_Demo
{
    public static class DependencyObjectExtensions
    {
        ///<summary>特定の型の先祖要素を取得</summary>
        public static T FindAncestor<T>(this DependencyObject startObject) where T : DependencyObject
        {
            var parent = startObject;
            while(parent != null)
            {
                if(typeof(T).IsInstanceOfType(parent)) break;
                else parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }

        // (c)じんぐる 2013
        // http://blog.xin9le.net/entry/2013/10/29/222336
        ///<summary>子要素を取得</summary>
        public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
        {
            if(obj == null) throw new ArgumentNullException("obj");

            var count = VisualTreeHelper.GetChildrenCount(obj);
            if(count == 0) yield break;

            for(var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if(child != null) yield return child;
            }
        }
        ///<summary>子孫要素を取得</summary>
        public static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
        {
            if(obj == null) throw new ArgumentNullException("obj");

            foreach(var child in obj.Children())
            {
                yield return child;
                foreach(var grandChild in child.Descendants())
                    yield return grandChild;
            }
        }
        ///<summary>特定の型の子要素を取得</summary>
        public static IEnumerable<T> Children<T>(this DependencyObject obj) where T : DependencyObject
            => obj.Children().OfType<T>();
        ///<summary>特定の型の子孫要素を取得</summary>
        public static IEnumerable<T> Descendants<T>(this DependencyObject obj) where T : DependencyObject
            => obj.Descendants().OfType<T>();
    }
}
