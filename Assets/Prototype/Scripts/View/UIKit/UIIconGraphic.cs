using UnityEngine;
using UnityEngine.UI;

namespace Nemequene.UI
{
    public enum UIIcon
    {
        Diamond, Sun, Heart, Sword, Shield, Dodge, Close, Check, Portal, Divider, Hourglass, Mask, Radiant,
        Settings, Alert, Bird, Bag, Lock, Journal, Disc, Rotate, Save, Guardian, Info, Player, Moon, Hand,
        Map, Objective, Eye, Pause, Poporo, Serpent, Vessel, Back, Voice, Freeze
    }

    // Line icons of the Bacatá kit (Resources/Nemequene/Bacata/Icons, white so the colour tints
    // them gold, bone or crimson). Diamond and Divider stay vector ornaments. Icons label states
    // alongside written text; no icon is the only carrier of meaning.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class UIIconGraphic : MaskableGraphic
    {
        public UIIcon icon;
        private Vector2 _center;
        private float _scale, _angle;
        private Sprite _sprite;
        private UIIcon _spriteFor = (UIIcon)(-1);
        public void SetIcon(UIIcon value) { if (icon == value) return; icon = value; SetVerticesDirty(); SetMaterialDirty(); }
        public static string SpriteName(UIIcon icon)
        {
            switch (icon)
            {
                case UIIcon.Sun: case UIIcon.Radiant: return "sol";
                case UIIcon.Heart: return "vida";
                case UIIcon.Sword: return "atacar";
                case UIIcon.Shield: return "bloquear";
                case UIIcon.Dodge: return "esquivar";
                case UIIcon.Close: return "cerrar";
                case UIIcon.Check: return "check";
                case UIIcon.Portal: return "portal";
                case UIIcon.Hourglass: return "girar";
                case UIIcon.Mask: return "alerta";
                case UIIcon.Settings: return "ajustes";
                case UIIcon.Alert: return "alerta";
                case UIIcon.Bird: return "ave";
                case UIIcon.Bag: return "bolsa";
                case UIIcon.Lock: return "candado";
                case UIIcon.Journal: return "diario";
                case UIIcon.Disc: return "disco";
                case UIIcon.Rotate: return "girar";
                case UIIcon.Save: return "guardar";
                case UIIcon.Guardian: return "guardian";
                case UIIcon.Info: return "info";
                case UIIcon.Player: return "jugador";
                case UIIcon.Moon: return "luna";
                case UIIcon.Hand: return "mano";
                case UIIcon.Map: return "mapa";
                case UIIcon.Objective: return "objetivo";
                case UIIcon.Eye: return "ojo";
                case UIIcon.Pause: return "pausa";
                case UIIcon.Poporo: return "poporo";
                case UIIcon.Serpent: return "serpiente";
                case UIIcon.Vessel: return "vasija";
                case UIIcon.Back: return "volver";
                case UIIcon.Voice: return "voz";
                case UIIcon.Freeze: return "congelar";
                default: return null;
            }
        }
        private Sprite IconSprite
        {
            get
            {
                if (_spriteFor != icon) { _spriteFor = icon; var name = SpriteName(icon); _sprite = name == null ? null : UIBacata.Get("Icons/" + name); }
                return _sprite;
            }
        }
        public override Texture mainTexture => IconSprite != null ? IconSprite.texture : base.mainTexture;
        private Vector2 P(float x, float y)
        {
            float cos = Mathf.Cos(_angle), sin = Mathf.Sin(_angle);
            return _center + new Vector2(x * cos - y * sin, x * sin + y * cos) * _scale;
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            if (r.width < 2 || r.height < 2) return;
            var tone = color; var dark = UIPalette.Charcoal; dark.a = tone.a;
            _center = r.center; _scale = Mathf.Min(r.width, r.height) * .5f; _angle = 0;
            var sprite = IconSprite;
            if (sprite != null)
            {
                // Square line icon centred in the rect; UVs come from the sprite (single, full texture).
                var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
                float half = _scale; int i = mesh.currentVertCount;
                mesh.AddVert(_center + new Vector2(-half, -half), tone, new Vector2(uv.x, uv.y));
                mesh.AddVert(_center + new Vector2(-half, half), tone, new Vector2(uv.x, uv.w));
                mesh.AddVert(_center + new Vector2(half, half), tone, new Vector2(uv.z, uv.w));
                mesh.AddVert(_center + new Vector2(half, -half), tone, new Vector2(uv.z, uv.y));
                mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
                return;
            }
            switch (icon)
            {
                case UIIcon.Divider:
                {
                    float y = r.center.y, x0 = r.xMin + 6, x1 = r.xMax - 6, gap = 14;
                    UIPlateGraphic.Line(mesh, new Vector2(x0, y), new Vector2(r.center.x - gap, y), 1.5f, tone);
                    UIPlateGraphic.Line(mesh, new Vector2(r.center.x + gap, y), new Vector2(x1, y), 1.5f, tone);
                    UIPlateGraphic.Diamond(mesh, r.center, 7, tone); UIPlateGraphic.Diamond(mesh, r.center, 3.5f, dark);
                    UIPlateGraphic.Diamond(mesh, new Vector2(x0, y), 3.5f, tone); UIPlateGraphic.Diamond(mesh, new Vector2(x1, y), 3.5f, tone);
                    break;
                }
                case UIIcon.Diamond:
                    Poly(mesh, tone, P(0, 1), P(1, 0), P(0, -1), P(-1, 0));
                    Poly(mesh, dark, P(0, .68f), P(.68f, 0), P(0, -.68f), P(-.68f, 0));
                    Poly(mesh, tone, P(0, .36f), P(.36f, 0), P(0, -.36f), P(-.36f, 0));
                    break;
                case UIIcon.Sun:
                    for (int i = 0; i < 12; i++)
                    {
                        _angle = i * Mathf.PI / 6;
                        Poly(mesh, tone, P(-.13f, .6f), P(0, 1), P(.13f, .6f));
                    }
                    _angle = 0;
                    Disc(mesh, _center, _scale * .66f, tone); Disc(mesh, _center, _scale * .54f, dark);
                    Poly(mesh, tone, P(-.08f, .36f), P(.08f, .36f), P(.05f, -.1f), P(-.05f, -.1f));
                    Poly(mesh, tone, P(0, -.2f), P(.09f, -.29f), P(0, -.38f), P(-.09f, -.29f));
                    break;
                case UIIcon.Radiant:
                    for (int i = 0; i < 16; i++)
                    {
                        _angle = i * Mathf.PI / 8;
                        Poly(mesh, tone, P(-.1f, .62f), P(0, i % 2 == 0 ? 1 : .82f), P(.1f, .62f));
                    }
                    _angle = 0;
                    Disc(mesh, _center, _scale * .6f, tone); Disc(mesh, _center, _scale * .48f, dark);
                    Ring(mesh, _center, _scale * .32f, _scale * .06f, tone);
                    UIPlateGraphic.Diamond(mesh, _center, _scale * .14f, tone);
                    break;
                case UIIcon.Heart:
                    Disc(mesh, P(-.42f, .28f), _scale * .46f, tone); Disc(mesh, P(.42f, .28f), _scale * .46f, tone);
                    Poly(mesh, tone, P(-.86f, .14f), P(.86f, .14f), P(0, -.9f));
                    Disc(mesh, P(-.42f, .36f), _scale * .14f, new Color(1, 1, 1, .28f * tone.a));
                    break;
                case UIIcon.Sword:
                    _angle = -Mathf.PI / 4;
                    Poly(mesh, tone, P(-.1f, -.42f), P(.1f, -.42f), P(.1f, .72f), P(0, .96f), P(-.1f, .72f));
                    Poly(mesh, tone, P(-.46f, -.5f), P(.46f, -.5f), P(.46f, -.36f), P(-.46f, -.36f));
                    Poly(mesh, tone, P(-.06f, -.5f), P(.06f, -.5f), P(.06f, -.84f), P(-.06f, -.84f));
                    Poly(mesh, tone, P(0, -.8f), P(.12f, -.92f), P(0, -1.04f), P(-.12f, -.92f));
                    break;
                case UIIcon.Shield:
                    Poly(mesh, tone, P(-.76f, .82f), P(.76f, .82f), P(.76f, .08f), P(0, -.96f), P(-.76f, .08f));
                    Poly(mesh, dark, P(-.56f, .64f), P(.56f, .64f), P(.56f, .12f), P(0, -.68f), P(-.56f, .12f));
                    Poly(mesh, tone, P(0, .36f), P(.22f, .1f), P(0, -.16f), P(-.22f, .1f));
                    break;
                case UIIcon.Dodge:
                    for (int i = 0; i < 2; i++)
                    {
                        float x = -.5f + i * .6f;
                        UIPlateGraphic.Line(mesh, P(x, .6f), P(x + .42f, 0), _scale * .16f, tone);
                        UIPlateGraphic.Line(mesh, P(x + .42f, 0), P(x, -.6f), _scale * .16f, tone);
                    }
                    break;
                case UIIcon.Close:
                    UIPlateGraphic.Line(mesh, P(-.6f, .6f), P(.6f, -.6f), _scale * .16f, tone);
                    UIPlateGraphic.Line(mesh, P(-.6f, -.6f), P(.6f, .6f), _scale * .16f, tone);
                    break;
                case UIIcon.Check:
                    UIPlateGraphic.Line(mesh, P(-.66f, .02f), P(-.18f, -.46f), _scale * .18f, tone);
                    UIPlateGraphic.Line(mesh, P(-.18f, -.46f), P(.7f, .56f), _scale * .18f, tone);
                    break;
                case UIIcon.Portal:
                    Ring(mesh, _center, _scale * .92f, _scale * .14f, tone);
                    Ring(mesh, _center, _scale * .52f, _scale * .08f, tone);
                    UIPlateGraphic.Diamond(mesh, _center, _scale * .2f, tone);
                    break;
                case UIIcon.Hourglass:
                    Poly(mesh, tone, P(-.6f, .86f), P(.6f, .86f), P(.08f, 0), P(-.08f, 0));
                    Poly(mesh, tone, P(-.08f, 0), P(.08f, 0), P(.6f, -.86f), P(-.6f, -.86f));
                    break;
                case UIIcon.Mask:
                    Poly(mesh, tone, P(-.82f, .9f), P(.82f, .9f), P(.82f, .1f), P(.36f, -.62f), P(0, -.94f), P(-.36f, -.62f), P(-.82f, .1f));
                    Poly(mesh, dark, P(-.6f, .44f), P(-.16f, .44f), P(-.16f, .18f), P(-.6f, .26f));
                    Poly(mesh, dark, P(.16f, .44f), P(.6f, .44f), P(.6f, .26f), P(.16f, .18f));
                    Poly(mesh, dark, P(-.28f, -.34f), P(.28f, -.34f), P(.16f, -.48f), P(-.16f, -.48f));
                    Poly(mesh, dark, P(0, .8f), P(.1f, .66f), P(0, .52f), P(-.1f, .66f));
                    break;
            }
        }
        private static void Poly(VertexHelper mesh, Color color, params Vector2[] points)
        {
            int start = mesh.currentVertCount;
            foreach (var p in points) mesh.AddVert(p, color, Vector2.zero);
            for (int i = 1; i < points.Length - 1; i++) mesh.AddTriangle(start, start + i, start + i + 1);
        }
        private static void Disc(VertexHelper mesh, Vector2 center, float radius, Color color)
        {
            int start = mesh.currentVertCount; const int segments = 24;
            mesh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                mesh.AddVert(center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, color, Vector2.zero);
            }
            for (int i = 0; i < segments; i++) mesh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % segments);
        }
        private static void Ring(VertexHelper mesh, Vector2 center, float radius, float width, Color color)
        {
            const int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                UIPlateGraphic.Line(mesh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, width, color);
            }
        }
    }
}
