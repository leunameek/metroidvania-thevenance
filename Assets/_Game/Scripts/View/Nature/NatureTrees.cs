using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// The trees and shrubs of the Andean savanna, built from code like the grass (2026-10-07: no
// models were needed). Each species grows its crown first — leaf clusters spread through an
// ellipsoid — then limbs from the trunk reach toward groups of clusters and twigs join each
// cluster to its limb, so the shape always reads as the species. Leaves are small two-triangle
// blades (palmate fans for the mano de oso, hanging strands for the sauce, arching culms for the
// chusque); their normals point out of the crown so the canopy shades as one soft mass. Three
// variants of each are built once and drawn instanced (Stylized Tree), casting shadows.
public static class NatureTrees
{
    public enum Form { Tree, Weeping, Palmate, Shrub, Bamboo }

    public sealed class Species
    {
        public string Key;
        public Form Form = Form.Tree;
        public float Height = 8, TrunkRadius = .2f, LimbFrom = .3f, LimbTo = .4f;
        public int Stems = 1, Limbs = 6, Clusters = 36, LeavesPerCluster = 40;
        public float CrownWidth = 2.5f, CrownLow = .3f, ClusterRadius = .8f, LeafSize = .15f, LeafWidth = .5f, Rise = .3f;
        public Color Bark = new Color(.42f, .33f, .25f), BarkDark = new Color(.24f, .18f, .13f);
        public Color LeafDark = new Color(.13f, .26f, .12f), LeafLight = new Color(.42f, .58f, .22f), Accent = new Color(.7f, .28f, .18f);
        public float AccentAmount, Wind = .25f, Flutter = .02f, Translucency = .5f;
        public bool Collider => Height > 3f;
    }

    // Heights as in the model list given to the art side (BacataModelSetup's Fit.Height).
    public static readonly Dictionary<string, Species> All = new Dictionary<string, Species>
    {
        ["Aliso"] = new Species
        {
            Key = "Aliso", Height = 10, TrunkRadius = .2f, LimbFrom = .3f, LimbTo = .78f, Limbs = 8, Clusters = 56, LeavesPerCluster = 60,
            CrownWidth = 2.5f, CrownLow = .3f, ClusterRadius = 1.05f, LeafSize = .34f, LeafWidth = .5f, Rise = .45f,
            Bark = new Color(.56f, .53f, .48f), BarkDark = new Color(.30f, .28f, .25f),
            LeafDark = new Color(.11f, .25f, .13f), LeafLight = new Color(.36f, .54f, .22f), Wind = .28f,
        },
        ["Roble"] = new Species
        {
            Key = "Roble", Height = 13, TrunkRadius = .4f, LimbFrom = .28f, LimbTo = .4f, Limbs = 6, Clusters = 78, LeavesPerCluster = 60,
            CrownWidth = 4.8f, CrownLow = .3f, ClusterRadius = 1.45f, LeafSize = .38f, LeafWidth = .48f, Rise = .35f,
            Bark = new Color(.40f, .31f, .24f), BarkDark = new Color(.19f, .14f, .10f),
            LeafDark = new Color(.09f, .21f, .09f), LeafLight = new Color(.32f, .48f, .17f), Accent = new Color(.56f, .44f, .2f), AccentAmount = .07f, Wind = .22f,
        },
        ["Encenillo"] = new Species
        {
            Key = "Encenillo", Height = 6, TrunkRadius = .1f, Stems = 3, LimbFrom = .22f, LimbTo = .5f, Limbs = 7, Clusters = 46, LeavesPerCluster = 60,
            CrownWidth = 2.1f, CrownLow = .22f, ClusterRadius = .82f, LeafSize = .22f, LeafWidth = .5f, Rise = .4f,
            Bark = new Color(.44f, .36f, .30f), BarkDark = new Color(.22f, .17f, .14f),
            LeafDark = new Color(.07f, .19f, .10f), LeafLight = new Color(.28f, .44f, .18f), Accent = new Color(.62f, .30f, .20f), AccentAmount = .1f, Wind = .26f,
        },
        ["Sauce"] = new Species
        {
            Key = "Sauce", Form = Form.Weeping, Height = 8, TrunkRadius = .26f, LimbFrom = .34f, LimbTo = .5f, Limbs = 7, Clusters = 40, LeavesPerCluster = 34,
            CrownWidth = 3.2f, CrownLow = .38f, ClusterRadius = .5f, LeafSize = .22f, LeafWidth = .24f, Rise = .7f,
            Bark = new Color(.42f, .36f, .28f), BarkDark = new Color(.22f, .18f, .13f),
            LeafDark = new Color(.20f, .32f, .14f), LeafLight = new Color(.52f, .66f, .30f), Wind = .32f, Flutter = .03f,
        },
        ["ManoDeOso"] = new Species
        {
            Key = "ManoDeOso", Form = Form.Palmate, Height = 5, TrunkRadius = .14f, LimbFrom = .42f, LimbTo = .6f, Limbs = 5, Clusters = 14, LeavesPerCluster = 9,
            CrownWidth = 1.8f, CrownLow = .5f, ClusterRadius = .25f, LeafSize = .5f, LeafWidth = 1, Rise = .9f,
            Bark = new Color(.50f, .44f, .36f), BarkDark = new Color(.26f, .22f, .17f),
            LeafDark = new Color(.10f, .22f, .11f), LeafLight = new Color(.30f, .46f, .20f), Accent = new Color(.62f, .46f, .28f), AccentAmount = .18f, Wind = .2f, Flutter = .015f,
        },
        ["Chilco"] = new Species
        {
            Key = "Chilco", Form = Form.Shrub, Height = 1.4f, TrunkRadius = .03f, Limbs = 8, Clusters = 20, LeavesPerCluster = 60,
            CrownWidth = .9f, CrownLow = .2f, ClusterRadius = .38f, LeafSize = .13f, LeafWidth = .45f, Rise = .5f,
            Bark = new Color(.42f, .36f, .26f), BarkDark = new Color(.22f, .19f, .14f),
            LeafDark = new Color(.17f, .31f, .13f), LeafLight = new Color(.48f, .62f, .28f), Accent = new Color(.94f, .92f, .78f), AccentAmount = .12f, Wind = .16f,
        },
        ["Mortino"] = new Species
        {
            Key = "Mortino", Form = Form.Shrub, Height = 1.1f, TrunkRadius = .025f, Limbs = 7, Clusters = 16, LeavesPerCluster = 56,
            CrownWidth = .75f, CrownLow = .2f, ClusterRadius = .33f, LeafSize = .11f, LeafWidth = .55f, Rise = .5f,
            Bark = new Color(.40f, .30f, .24f), BarkDark = new Color(.2f, .15f, .12f),
            LeafDark = new Color(.10f, .22f, .10f), LeafLight = new Color(.34f, .50f, .20f), Accent = new Color(.78f, .10f, .16f), AccentAmount = .14f, Wind = .16f,
        },
        ["Chusque"] = new Species
        {
            Key = "Chusque", Form = Form.Bamboo, Height = 2.6f, TrunkRadius = .018f, Limbs = 22, LeavesPerCluster = 5,
            CrownWidth = 1.4f, CrownLow = .25f, LeafSize = .26f, LeafWidth = .14f,
            Bark = new Color(.62f, .60f, .32f), BarkDark = new Color(.38f, .40f, .20f),
            LeafDark = new Color(.26f, .38f, .14f), LeafLight = new Color(.62f, .72f, .30f), Wind = .3f, Flutter = .03f,
        },
    };

    public const int Variants = 3;
    private static readonly Dictionary<string, Mesh> Meshes = new Dictionary<string, Mesh>();
    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    public static bool Has(string key) => All.ContainsKey(key);

    public static Mesh Mesh(string key, int variant)
    {
        string id = key + "#" + variant;
        if (Meshes.TryGetValue(id, out var mesh) && mesh != null) return mesh;
        mesh = new Builder(All[key], 1000 + variant * 37 + Seed(key)).Build();
        mesh.name = id;
        return Meshes[id] = mesh;
    }

    private static int Seed(string key) { int h = 0; foreach (char ch in key) h = h * 31 + ch; return (h & 0x7fffffff) % 997; }

    public static Material Material(string key)
    {
        if (Materials.TryGetValue(key, out var m) && m != null) return m;
        var shader = NatureKit.Shader("StylizedTree", "Nemequene/Stylized Tree");
        if (shader == null) { Debug.LogWarning("Falta el shader Nemequene/Stylized Tree"); return null; }
        var s = All[key];
        m = new Material(shader) { name = "Arbol_" + key, enableInstancing = true };
        m.SetColor("_Bark", s.Bark); m.SetColor("_BarkDark", s.BarkDark);
        m.SetColor("_LeafDark", s.LeafDark); m.SetColor("_LeafLight", s.LeafLight);
        m.SetColor("_LeafAccent", s.Accent); m.SetFloat("_AccentAmount", s.AccentAmount);
        m.SetFloat("_WindStrength", s.Wind); m.SetFloat("_Flutter", s.Flutter); m.SetFloat("_Translucency", s.Translucency);
        return Materials[key] = m;
    }

    // ---------------------------------------------------------------- a planted forest

    // Collects trees as they are planted, then builds one instanced field per species variant and
    // a trunk collider for each real tree.
    public sealed class Forest
    {
        private readonly Dictionary<string, List<Matrix4x4>> _fields = new Dictionary<string, List<Matrix4x4>>();
        private readonly List<(Species s, Vector3 at, float scale)> _trunks = new List<(Species, Vector3, float)>();
        private int _count;

        public bool Add(string key, Vector3 at, float yaw, float scale)
        {
            if (!All.TryGetValue(key, out var s)) return false;
            int variant = (int)(Mathf.Abs(at.x * 7.13f + at.z * 3.71f)) % Variants;
            string id = key + "#" + variant;
            if (!_fields.TryGetValue(id, out var list)) _fields[id] = list = new List<Matrix4x4>();
            list.Add(Matrix4x4.TRS(at, Quaternion.Euler(0, yaw, 0), Vector3.one * scale));
            if (s.Collider) _trunks.Add((s, at, scale));
            _count++;
            return true;
        }

        public int Count => _count;

        public Transform Build(Transform parent, string name = "Arboles")
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            foreach (var pair in _fields)
            {
                string key = pair.Key.Substring(0, pair.Key.IndexOf('#'));
                int variant = int.Parse(pair.Key.Substring(pair.Key.IndexOf('#') + 1));
                var material = Material(key);
                if (material == null) continue;
                NatureGrass.Create(root, pair.Key, Mesh(key, variant), material, pair.Value, true);
            }
            foreach (var (s, at, scale) in _trunks)
            {
                var trunk = new GameObject("Tronco " + s.Key);
                trunk.transform.SetParent(root, false);
                trunk.transform.position = at;
                var c = trunk.AddComponent<CapsuleCollider>();
                c.radius = Mathf.Max(.15f, s.TrunkRadius * scale * (s.Stems > 1 ? 2.2f : 1.2f));
                c.height = s.Height * scale * .45f;
                c.center = Vector3.up * c.height * .5f;
            }
            return root;
        }
    }

    // ---------------------------------------------------------------- the generator

    private sealed class Builder
    {
        private readonly Species _s;
        private readonly System.Random _random;
        private readonly List<Vector3> _v = new List<Vector3>(), _n = new List<Vector3>();
        private readonly List<Color> _c = new List<Color>();
        private readonly List<int> _t = new List<int>();
        private Vector3 _centre, _radii;

        public Builder(Species s, int seed) { _s = s; _random = new System.Random(seed); }

        private float R() => (float)_random.NextDouble();
        private float R(float a, float b) => a + (b - a) * R();
        private Vector3 Sphere() { Vector3 p; do p = new Vector3(R() * 2 - 1, R() * 2 - 1, R() * 2 - 1); while (p.sqrMagnitude > 1 || p.sqrMagnitude < .01f); return p; }

        public Mesh Build()
        {
            float h = _s.Height;
            float low = h * _s.CrownLow;
            _centre = new Vector3(0, (low + h) * .5f, 0);
            _radii = new Vector3(_s.CrownWidth, (h - low) * .5f, _s.CrownWidth);
            if (_s.Form == Form.Bamboo) Bamboo();
            else Crowned();
            var mesh = new Mesh();
            if (_v.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(_v); mesh.SetNormals(_n); mesh.SetColors(_c); mesh.SetTriangles(_t, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // How far out in the crown a point is (0 centre, 1 skin), lighter toward the top.
        private float Open(Vector3 p)
        {
            var q = new Vector3((p.x - _centre.x) / _radii.x, (p.y - _centre.y) / _radii.y, (p.z - _centre.z) / _radii.z);
            return Mathf.Clamp01(Mathf.SmoothStep(.25f, 1.05f, q.magnitude) + q.y * .15f);
        }

        private Vector3 Out(Vector3 p)
        {
            var q = new Vector3((p.x - _centre.x) / (_radii.x * _radii.x), (p.y - _centre.y) / (_radii.y * _radii.y), (p.z - _centre.z) / (_radii.z * _radii.z));
            return q.sqrMagnitude > 1e-6f ? q.normalized : Vector3.up;
        }

        private float Flex(Vector3 p) => Mathf.Clamp01(p.y / _s.Height);

        // ------------------------------------------------------------ crowned trees and shrubs
        private void Crowned()
        {
            var s = _s;
            bool shrub = s.Form == Form.Shrub;
            // 1. The crown: clusters through the ellipsoid, most of them near its skin.
            var clusters = new List<Vector3>();
            for (int i = 0; i < s.Clusters; i++)
            {
                var d = Sphere().normalized * Mathf.Lerp(.5f, .95f, Mathf.Sqrt(R()));
                if (s.Form == Form.Weeping) d.y = Mathf.Abs(d.y) * .8f + .1f; // the willow's clusters sit high, the strands hang
                if (s.Form == Form.Palmate) d.y = Mathf.Abs(d.y) * .6f + .35f;
                clusters.Add(_centre + Vector3.Scale(d, _radii));
            }

            // 2. Stems from the ground.
            var stems = new List<List<Vector3>>();
            float trunkTop = s.Height * s.LimbTo;
            if (!shrub)
            {
                for (int k = 0; k < s.Stems; k++)
                {
                    float a = (k + R()) / s.Stems * Mathf.PI * 2;
                    var lean = s.Stems > 1 ? new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * s.Height * .09f : new Vector3(R() - .5f, 0, R() - .5f) * s.Height * .04f;
                    var path = new List<Vector3>();
                    for (int i = 0; i <= 6; i++)
                    {
                        float u = i / 6f;
                        var wobble = new Vector3(R() - .5f, 0, R() - .5f) * s.TrunkRadius * .8f * (i > 0 ? 1 : 0);
                        path.Add(lean * Mathf.Pow(u, .8f) + Vector3.up * trunkTop * u + wobble);
                    }
                    float r = s.TrunkRadius * (s.Stems > 1 ? .8f : 1f);
                    Tube(path, r * 1.35f, r * .45f, 0, .18f, 8, R(), true);
                    stems.Add(path);
                }
            }

            // 3. Limbs: each reaches toward its sector of the crown.
            var targets = new List<Vector3>(); var starts = new List<Vector3>();
            for (int i = 0; i < s.Limbs; i++)
            {
                float a = (i + R() * .7f) / s.Limbs * Mathf.PI * 2;
                float y = Mathf.Lerp(-.35f, .65f, (i % 3 + R()) / 3f);
                var dir = new Vector3(Mathf.Cos(a), y, Mathf.Sin(a));
                targets.Add(_centre + Vector3.Scale(dir.normalized * .62f, _radii));
                if (shrub) starts.Add(new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * s.CrownWidth * .12f);
                else
                {
                    var stem = stems[i % stems.Count];
                    float from = s.LimbFrom / s.LimbTo;
                    // Lower limbs leave the trunk lower: the trunk point follows the target's height.
                    float u = Mathf.Lerp(from, 1f, Mathf.Clamp01((targets[i].y - s.Height * s.CrownLow) / (s.Height * (1 - s.CrownLow)) + R() * .2f));
                    starts.Add(PointOn(stem, u));
                }
            }
            var groups = new List<Vector3>[s.Limbs];
            for (int i = 0; i < s.Limbs; i++) groups[i] = new List<Vector3>();
            foreach (var c in clusters)
            {
                int best = 0; float bestD = float.MaxValue;
                for (int i = 0; i < s.Limbs; i++) { float d = (targets[i] - c).sqrMagnitude; if (d < bestD) { bestD = d; best = i; } }
                groups[best].Add(c);
            }
            float limbRadius = shrub ? s.TrunkRadius : s.TrunkRadius * .5f;
            for (int i = 0; i < s.Limbs; i++)
            {
                if (groups[i].Count == 0) continue;
                var centroid = Vector3.zero; foreach (var c in groups[i]) centroid += c; centroid /= groups[i].Count;
                var start = starts[i];
                var end = Vector3.Lerp(start, centroid, .78f);
                var limb = Curve(start, end, s.Rise, 6);
                float f0 = Flex(start) * .5f + (shrub ? .05f : .12f);
                Tube(limb, limbRadius, limbRadius * .35f, f0, .5f, shrub ? 4 : 6, R(), false);
                // 4. Twigs from the limb to each cluster, then the leaves.
                foreach (var c in groups[i])
                {
                    var from = Nearest(limb, c, .4f);
                    var twig = Curve(from, c, .25f, 4);
                    Tube(twig, limbRadius * .3f, limbRadius * .08f, .45f, .7f, 3, R(), false);
                    if (s.Form == Form.Weeping) Strands(c);
                    else if (s.Form == Form.Palmate) Rosette(c);
                    else Cluster(c);
                }
            }
        }

        private static Vector3 PointOn(List<Vector3> path, float u)
        {
            float f = Mathf.Clamp01(u) * (path.Count - 1);
            int i = Mathf.Min(path.Count - 2, (int)f);
            return Vector3.Lerp(path[i], path[i + 1], f - i);
        }

        private static Vector3 Nearest(List<Vector3> path, Vector3 p, float from)
        {
            Vector3 best = path[path.Count - 1]; float bestD = float.MaxValue;
            for (int i = Mathf.FloorToInt(from * (path.Count - 1)); i < path.Count; i++)
            {
                float d = (path[i] - p).sqrMagnitude;
                if (d < bestD) { bestD = d; best = path[i]; }
            }
            return best;
        }

        // A bent path from a to b: it rises first and arrives from above (rise = how much).
        private List<Vector3> Curve(Vector3 a, Vector3 b, float rise, int points)
        {
            float length = (b - a).magnitude;
            var control = a + (b - a) * .35f + Vector3.up * length * rise + new Vector3(R() - .5f, 0, R() - .5f) * length * .15f;
            var path = new List<Vector3>();
            for (int i = 0; i <= points; i++)
            {
                float t = i / (float)points;
                path.Add((1 - t) * (1 - t) * a + 2 * (1 - t) * t * control + t * t * b);
            }
            return path;
        }

        // ------------------------------------------------------------ leaves
        private void Cluster(Vector3 c)
        {
            var s = _s;
            var outDir = Out(c);
            for (int i = 0; i < s.LeavesPerCluster; i++)
            {
                var p = c + (Sphere() * .7f + outDir * .45f) * s.ClusterRadius;
                var normal = (Out(p) + Sphere() * .6f).normalized;
                var along = Vector3.Cross(normal, Sphere()).normalized;
                if (along.sqrMagnitude < .1f) along = Vector3.forward;
                // Leaves droop a little: their tips lean down and out.
                along = (along + Vector3.down * .25f + Out(p) * .3f).normalized;
                Leaf(p, along, normal, s.LeafSize * R(.75f, 1.25f), s.LeafWidth);
            }
        }

        // The willow: long thin twigs hanging from each cluster, slim leaves all along them.
        private void Strands(Vector3 c)
        {
            var s = _s;
            for (int k = 0; k < 4; k++)
            {
                var start = c + Sphere() * s.ClusterRadius;
                float length = R(1.2f, 2.8f) * s.Height / 8f;
                var outward = Out(start); outward.y = 0; outward = outward.normalized;
                var path = new List<Vector3>();
                for (int i = 0; i <= 5; i++)
                {
                    float u = i / 5f;
                    path.Add(start + outward * length * .22f * Mathf.Sin(u * 1.4f) + Vector3.down * length * u * u * .95f + Vector3.down * length * u * .05f);
                }
                Tube(path, .012f, .004f, .6f, 1f, 3, R(), false);
                for (int i = 0; i < s.LeavesPerCluster; i++)
                {
                    float u = (i + R()) / s.LeavesPerCluster;
                    var p = PointOn(path, u);
                    var side = Quaternion.AngleAxis(R() * 360, Vector3.up) * Vector3.right;
                    var along = (side * .6f + Vector3.down).normalized;
                    var normal = (Out(p) + side * .3f).normalized;
                    Leaf(p, along, normal, s.LeafSize * R(.8f, 1.2f), s.LeafWidth);
                }
            }
        }

        // The mano de oso: a rosette of big hand-shaped leaves at each branch tip.
        private void Rosette(Vector3 c)
        {
            var s = _s;
            for (int i = 0; i < s.LeavesPerCluster; i++)
            {
                float a = (i + R() * .5f) / s.LeavesPerCluster * Mathf.PI * 2;
                var dir = new Vector3(Mathf.Cos(a), R(.05f, .45f), Mathf.Sin(a)).normalized;
                float size = s.LeafSize * R(.75f, 1.2f);
                var at = c + dir * size * .45f;
                var normal = (Vector3.up * 1.2f + dir * .5f + Sphere() * .2f).normalized;
                Palm(at, dir, normal, size);
            }
        }

        // A two-triangle leaf: base, sides at its widest, tip; folded a little along its vein.
        private void Leaf(Vector3 at, Vector3 along, Vector3 normal, float length, float width)
        {
            var side = Vector3.Cross(normal, along).normalized * length * width * .5f;
            var fold = normal * length * width * .12f;
            var mid = at + along * length * .42f;
            int b = _v.Count;
            Vector3 n = (Out(at) * .7f + normal * .3f).normalized;
            float g = R(), open = Open(at), flex = Mathf.Lerp(.6f, 1f, Flex(at));
            var color = new Color(1, g, open, flex);
            _v.Add(at); _v.Add(mid + side - fold); _v.Add(at + along * length); _v.Add(mid - side - fold); _v.Add(mid + fold * .5f);
            for (int i = 0; i < 5; i++) { _n.Add(n); _c.Add(color); }
            _t.AddRange(new[] { b, b + 1, b + 4, b + 4, b + 1, b + 2, b, b + 4, b + 3, b + 4, b + 2, b + 3 });
        }

        // A hand-shaped leaf: seven lobes fanned out from the stalk.
        private void Palm(Vector3 at, Vector3 dir, Vector3 normal, float size)
        {
            var forward = Vector3.ProjectOnPlane(dir, normal).normalized;
            var side = Vector3.Cross(normal, forward);
            int centre = _v.Count;
            Vector3 n = (Out(at) * .6f + normal * .4f).normalized;
            float g = R(), open = Open(at), flex = Mathf.Lerp(.6f, 1f, Flex(at));
            var color = new Color(1, g, open, flex);
            _v.Add(at - forward * size * .1f); _n.Add(n); _c.Add(color);
            const int lobes = 7;
            for (int k = 0; k <= lobes * 2; k++)
            {
                float a = Mathf.Lerp(-110f, 110f, k / (lobes * 2f)) * Mathf.Deg2Rad;
                float r = size * (k % 2 == 1 ? 1f : .45f) * (1 - .25f * Mathf.Abs(a) / 1.9f);
                var p = at + (forward * Mathf.Cos(a) + side * Mathf.Sin(a)) * r - normal * r * .12f * (k % 2);
                _v.Add(p); _n.Add(n); _c.Add(color);
            }
            for (int k = 0; k < lobes * 2; k++) _t.AddRange(new[] { centre, centre + 1 + k, centre + 2 + k });
        }

        // ------------------------------------------------------------ the chusque
        private void Bamboo()
        {
            var s = _s;
            for (int i = 0; i < s.Limbs; i++)
            {
                float a = R() * Mathf.PI * 2, tilt = R(.05f, .5f);
                var outward = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                float length = s.Height * R(.7f, 1.1f);
                var basePoint = outward * R(0, .22f);
                var path = new List<Vector3>();
                for (int k = 0; k <= 8; k++)
                {
                    float u = k / 8f;
                    // Rises, then arches out under its own weight.
                    path.Add(basePoint + Vector3.up * length * (u - .28f * u * u * (tilt + .4f)) + outward * length * (tilt * u + .35f * u * u * (tilt + .3f)));
                }
                Tube(path, s.TrunkRadius, s.TrunkRadius * .4f, 0, 1, 4, R(), false);
                for (int k = 0; k < 9; k++)
                {
                    float u = Mathf.Lerp(.42f, 1f, k / 8f);
                    var p = PointOn(path, u);
                    for (int j = 0; j < s.LeavesPerCluster; j++)
                    {
                        float b = (j + R() * .6f) / s.LeavesPerCluster * Mathf.PI * 2;
                        var dir = (new Vector3(Mathf.Cos(b), -.35f, Mathf.Sin(b)) + outward * .4f).normalized;
                        var normal = (Vector3.up + Sphere() * .4f).normalized;
                        Leaf(p, dir, normal, s.LeafSize * R(.75f, 1.2f), s.LeafWidth);
                    }
                }
            }
        }

        // ------------------------------------------------------------ bark
        private void Tube(List<Vector3> path, float r0, float r1, float flex0, float flex1, int sides, float g, bool flare)
        {
            var tangent = (path[1] - path[0]).normalized;
            var u = Vector3.Cross(tangent, Mathf.Abs(tangent.y) < .9f ? Vector3.up : Vector3.right).normalized;
            int first = _v.Count;
            for (int i = 0; i < path.Count; i++)
            {
                float t = i / (float)(path.Count - 1);
                if (i > 0) tangent = (path[Mathf.Min(i + 1, path.Count - 1)] - path[i - 1]).normalized;
                u = (u - Vector3.Dot(u, tangent) * tangent).normalized;
                var v = Vector3.Cross(tangent, u);
                float r = Mathf.Lerp(r0, r1, t);
                if (flare && i == 0) r *= 1.35f; // the foot of the trunk spreads into the ground
                float flex = Mathf.Lerp(flex0, flex1, t);
                for (int k = 0; k < sides; k++)
                {
                    float a = k * Mathf.PI * 2 / sides;
                    var dir = u * Mathf.Cos(a) + v * Mathf.Sin(a);
                    _v.Add(path[i] + dir * r); _n.Add(dir);
                    _c.Add(new Color(0, g, Mathf.Lerp(.35f, .8f, Open(path[i])), flex));
                }
            }
            for (int i = 0; i < path.Count - 1; i++)
                for (int k = 0; k < sides; k++)
                {
                    int a = first + i * sides + k, b = first + i * sides + (k + 1) % sides;
                    int c = a + sides, d = b + sides;
                    _t.AddRange(new[] { a, c, b, b, c, d });
                }
        }
    }
}
