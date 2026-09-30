using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Nemequene.UI
{
    public sealed class MapUIController : IDisposable
    {
        private readonly UIManager _ui;
        private readonly HashSet<int> _visited = new HashSet<int>();
        private readonly HashSet<PlazaPortal> _knownPortals = new HashSet<PlazaPortal>();
        private readonly HashSet<AnalyzableObject> _knownObjects = new HashSet<AnalyzableObject>();
        private readonly TMP_Text _map;
        private readonly TMP_Text[] _archive;
        private readonly TMP_Text[] _archiveTitles;
        private readonly UIMapGraphic _graphic;
        private readonly Dictionary<int,List<Vector2>> _trails = new Dictionary<int,List<Vector2>>();
        private bool _combatKnown;
        public int VisitedWorlds => _visited.Count;
        public int VisitedMask => (_visited.Contains(-1) ? 1 : 0) | (_visited.Contains(0) ? 2 : 0) | (_visited.Contains(1) ? 4 : 0);
        public void RestoreVisitedMask(int mask)
        {
            if ((mask & 1) != 0) _visited.Add(-1);
            if ((mask & 2) != 0) _visited.Add(0);
            if ((mask & 4) != 0) _visited.Add(1);
            Discover();
        }
        public MapUIController(UIManager ui, MenuController menu)
        {
            _ui = ui; var body = menu.JournalPage(UIScreen.Map);
            var diagram = ui.Factory.Parchment("DiscoveredMap",body,Vector2.zero,Vector2.one);
            diagram.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=360;
            var drawing = ui.Factory.Rect("KnownPaths",diagram,new Vector2(.06f,.08f),new Vector2(.94f,.92f));
            _graphic=drawing.gameObject.AddComponent<UIMapGraphic>(); _graphic.raycastTarget=false;
            _map = ui.Factory.Text(body, "", 26);
            var archive = menu.JournalPage(UIScreen.Archive);
            _archive = new TMP_Text[ui.Demo.Objects.Length];
            _archiveTitles = new TMP_Text[_archive.Length];
            for (int i = 0; i < _archive.Length; i++)
            {
                var entry = ui.Factory.Column(archive, "Discovery", 12);
                _archiveTitles[i] = ui.Factory.Text(entry, "", 32, true);
                _archive[i] = ui.Factory.Text(entry, "", 24);
            }
            ui.Screens.Changed += Screen; ui.Demo.ViewChanged += Discover; ui.Demo.Objectives.Changed += Discover;
            ui.Settings.Changed += ApplyStyle; ApplyStyle();
            Discover();
        }
        private void ApplyStyle() { _graphic.SetContrast(_ui.Settings.Values.highContrast); }
        public void Discover()
        {
            if (_ui.SessionStarted) _visited.Add(_ui.Demo.World);
            if (_ui.SessionStarted && _ui.Demo.State==TechnicalDemoState.Exploration)
            {
                int world=_ui.Demo.World; Vector3 position=_ui.Demo.Player.transform.position;
                Vector2 point=new Vector2(position.x,position.z);
                if(!_trails.TryGetValue(world,out var trail)) { trail=new List<Vector2>(); _trails.Add(world,trail); }
                if(trail.Count==0 || Vector2.Distance(trail[trail.Count-1],point)>3)
                { if(trail.Count>=256) trail.RemoveAt(0); trail.Add(point); }
            }
            if (_ui.Demo.Nearby != null) _knownObjects.Add(_ui.Demo.Nearby);
            foreach (var item in _ui.Demo.Objects) if (item != null && item.Completed) _knownObjects.Add(item);
            if (_ui.Demo.NearbyPortal != null) _knownPortals.Add(_ui.Demo.NearbyPortal);
            if (_ui.Demo.NearCombat) _combatKnown = true;
        }
        private void Screen(UIScreen screen)
        {
            Discover();
            if (screen == UIScreen.Map)
            {
                string text = UIStrings.Get("map.current", UIStrings.Get(_ui.Demo.World < 0 ? "world.lower" : _ui.Demo.World > 0 ? "world.upper" : "world.plaza"));
                foreach (int world in new[] {0,-1,1})
                    if (_visited.Contains(world) && world != _ui.Demo.World) text += "\n\n" + UIStrings.Get("map.visited", UIStrings.Get(world < 0 ? "world.lower" : world > 0 ? "world.upper" : "world.plaza"));
                if (_knownObjects.Count > 0) text += "\n\n" + UIStrings.Get("map.objects");
                foreach (var item in _knownObjects) text += "\n  " + item.Data.displayName + " · " + UIStrings.Get(item.Completed ? "done" : "pending");
                if (_combatKnown) text += "\n\n" + UIStrings.Get("map.combat") + " · " + UIStrings.Get(_ui.Demo.Combat.Completed ? "done" : "pending");
                foreach (var portal in _knownPortals) text += "\n\n" + UIStrings.Get("map.path", portal.destinationName, UIStrings.Get(portal.Available ? "unlocked" : "locked"));
                _map.text = text;
                Vector3 player=_ui.Demo.Player.transform.position;
                var markers=new List<Vector2>();
                foreach(var item in _knownObjects)
                    if(Vector3.Distance(item.transform.position,player)<65) markers.Add(new Vector2(item.transform.position.x,item.transform.position.z));
                foreach(var portal in _knownPortals)
                    if(Vector3.Distance(portal.transform.position,player)<65) markers.Add(new Vector2(portal.transform.position.x,portal.transform.position.z));
                _graphic.SetMap(_trails.TryGetValue(_ui.Demo.World,out var route)?route:new List<Vector2>(),markers,new Vector2(player.x,player.z));
            }
            if (screen == UIScreen.Archive)
                for (int i = 0; i < _archive.Length; i++)
                {
                    var item = _ui.Demo.Objects[i];
                    bool known = item != null && item.Completed;
                    _archive[i].transform.parent.gameObject.SetActive(known || i == 0 && _ui.Demo.Objectives.AnalyzedCount == 0);
                    _archiveTitles[i].gameObject.SetActive(known);
                    _archiveTitles[i].text = known ? item.Data.displayName : "";
                    _archive[i].text = known ? InspectionUIController.CulturalDescription(item.Data) : UIStrings.Get("archive.unknown");
                }
        }
        public void Dispose()
        { _ui.Screens.Changed -= Screen; _ui.Settings.Changed -= ApplyStyle; if (_ui.Demo != null) { _ui.Demo.ViewChanged -= Discover; _ui.Demo.Objectives.Changed -= Discover; } }
    }
}
