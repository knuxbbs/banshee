//
// TrackListView.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gtk;

using Hyena.Data.Gui;

using Banshee.Gui;
using Banshee.Gui.DragDrop;
using Banshee.ServiceStack;
using Banshee.MediaEngine;
using Banshee.Collection;

namespace Banshee.Collection.Gui
{
    public class TrackListView : ListView<TrackInfo>
    {
        private ColumnController column_controller;
        
        public TrackListView() : base()
        {
            column_controller = new ColumnController();

            column_controller.AddRange(
                new Column(null, "indicator", new ColumnCellPlaybackIndicator(null), 0.05),
                new SortableColumn("Track", new ColumnCellTrackNumber("TrackNumber", true), 0.10, "Track"),
                new SortableColumn("Artist", new ColumnCellText("ArtistName", true), 0.225, "Artist"),
                new SortableColumn("Album", new ColumnCellText("AlbumTitle", true), 0.225, "Album"),
                new SortableColumn("Title", new ColumnCellText("TrackTitle", true), 0.25, "Title"),
                new SortableColumn("Duration", new ColumnCellDuration("Duration", true), 0.15, "Duration"),
                new SortableColumn("Year", new ColumnCellText("Year", true), 0.15, "Year"),
                new SortableColumn("Play Count", new ColumnCellText("PlayCount", true), 0.15, "PlayCount"),
                new SortableColumn("Skip Count", new ColumnCellText("SkipCount", true), 0.15, "SkipCount"),
                //new SortableColumn("Rating", new RatingColumnCell(null, true), 0.15, "Rating"),
                new SortableColumn("Last Played", new ColumnCellDateTime("LastPlayed", true), 0.15, "LastPlayedStamp"),
                new SortableColumn("Added", new ColumnCellDateTime("DateAdded", true), 0.15, "DateAddedStamp"),
                new SortableColumn("Location", new ColumnCellText("Uri", true), 0.15, "Uri")
            );
            
            ColumnController = DefaultColumnController;
            RulesHint = true;
            
            ServiceManager.PlayerEngine.StateChanged += OnPlayerEngineStateChanged;
            
            if (ServiceManager.Contains ("GtkElementsService")) {
                ServiceManager.Get<Banshee.Gui.GtkElementsService> ("GtkElementsService").ThemeChanged += delegate {
                    foreach (Column column in column_controller) {
                        if (column.HeaderCell != null) {
                            column.HeaderCell.NotifyThemeChange ();
                        }
                        
                        foreach (ColumnCell cell in column) {
                            cell.NotifyThemeChange ();
                        }
                    }
                    
                    QueueDraw ();
                };
            }
            
            ConfigureDragAndDrop ();
        }

        protected override bool OnPopupMenu ()
        {
            ServiceManager.Get<InterfaceActionService> ("InterfaceActionService").TrackActions["TrackContextMenuAction"].Activate ();
            return true;
        }
        
        private void OnPlayerEngineStateChanged (object o, PlayerEngineStateArgs args)
        {
            QueueDraw ();
        }
        
#region Drag and Drop

        private static TargetEntry [] dnd_source_entries = new TargetEntry [] {
            DragDropTarget.ModelSelection
        };

        private void ConfigureDragAndDrop ()
        {
            Gtk.Drag.SourceSet (this, Gdk.ModifierType.Button1Mask | Gdk.ModifierType.Button3Mask, 
                dnd_source_entries, Gdk.DragAction.Copy | Gdk.DragAction.Move);
                
            Drag.SourceSetIconName (this, "audio-x-generic");
        }

        protected override void OnDragDataGet (Gdk.DragContext context, SelectionData selection_data, uint info, uint time)
        {
            byte [] selection_data_raw;
            DragDropTargetType type = (DragDropTargetType)info;
            
            if (type != DragDropTargetType.ModelSelection || Selection.Count <= 0) {
                return;
            }
        }

#endregion
        
        public ColumnController DefaultColumnController {
            get { return column_controller; }
        }
    }
}
