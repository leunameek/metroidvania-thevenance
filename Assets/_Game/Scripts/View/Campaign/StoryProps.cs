using UnityEngine;

// Provisional pieces of the story built from primitives until their models exist (masks, coca,
// urns, the poporo, people). Each has a readable silhouette; a model placed under
// Resources/Props/<name> replaces it automatically.
public static class StoryProps
{
    public static Transform Build(string name, Transform parent, Vector3 position)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, true);
        root.position = position;
        var prefab = Resources.Load<GameObject>("Props/" + name);
        if (prefab != null) { Object.Instantiate(prefab, root, false); return root; }
        switch (name)
        {
            case "Coca": Coca(root); break;
            case "MascaraChia": Mask(root, new Color(.78f, .82f, .9f), new Color(.35f, .45f, .75f)); break;
            case "MascaraSue": Mask(root, new Color(.95f, .74f, .3f), new Color(.75f, .35f, .15f)); break;
            case "UrnaVacia": Urn(root, false); break;
            case "UrnaMemoria": Urn(root, true); break;
            case "Yopo": Bowl(root, new Color(.55f, .32f, .2f), new Color(.85f, .55f, .25f)); break;
            default: Part(PrimitiveType.Sphere, root, Vector3.zero, Vector3.one * .3f, Color.white); break;
        }
        return root;
    }

    // Three leaves of fantasy coca on a clay dish (no preparation shown).
    private static void Coca(Transform root)
    {
        Bowl(root, new Color(.5f, .3f, .18f), new Color(.6f, .38f, .2f));
        for (int i = 0; i < 3; i++)
        {
            var leaf = Part(PrimitiveType.Sphere, root, new Vector3(-.09f + .09f * i, .07f, 0), new Vector3(.08f, .015f, .18f), new Color(.25f, .55f, .2f));
            leaf.localRotation = Quaternion.Euler(0, -30 + 30 * i, 8);
        }
    }

    private static void Bowl(Transform root, Color clay, Color rim)
    {
        Part(PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(.32f, .04f, .32f), clay);
        Part(PrimitiveType.Cylinder, root, new Vector3(0, .03f, 0), new Vector3(.36f, .01f, .36f), rim);
    }

    // A funerary mask: face disc, brow, two eye slots and a nose ridge; the back shows the bond.
    private static void Mask(Transform root, Color metal, Color bond)
    {
        var face = Part(PrimitiveType.Cylinder, root, Vector3.zero, new Vector3(.42f, .02f, .5f), metal);
        face.localRotation = Quaternion.Euler(90, 0, 0);
        Part(PrimitiveType.Cube, root, new Vector3(0, .12f, -.03f), new Vector3(.34f, .04f, .03f), metal * .85f);
        Part(PrimitiveType.Cube, root, new Vector3(-.09f, .06f, -.03f), new Vector3(.09f, .03f, .02f), Color.black);
        Part(PrimitiveType.Cube, root, new Vector3(.09f, .06f, -.03f), new Vector3(.09f, .03f, .02f), Color.black);
        Part(PrimitiveType.Cube, root, new Vector3(0, -.03f, -.035f), new Vector3(.04f, .12f, .03f), metal * .9f);
        var mark = Part(PrimitiveType.Cube, root, new Vector3(0, 0, .03f), new Vector3(.3f, .02f, .01f), bond);
        mark.localRotation = Quaternion.Euler(0, 0, 35);
    }

    // Funerary urn; the empty one keeps a horn-shaped support inside.
    private static void Urn(Transform root, bool memory)
    {
        Part(PrimitiveType.Cylinder, root, new Vector3(0, .35f, 0), new Vector3(.55f, .35f, .55f), new Color(.6f, .36f, .22f));
        Part(PrimitiveType.Sphere, root, new Vector3(0, .62f, 0), new Vector3(.6f, .3f, .6f), new Color(.64f, .4f, .25f));
        Part(PrimitiveType.Cylinder, root, new Vector3(0, .82f, 0), new Vector3(.3f, .06f, .3f), new Color(.45f, .27f, .17f));
        if (memory) Part(PrimitiveType.Sphere, root, new Vector3(0, .8f, 0), new Vector3(.2f, .08f, .2f), new Color(.85f, .82f, .72f));
        else
        {
            var support = Part(PrimitiveType.Capsule, root, new Vector3(0, .82f, 0), new Vector3(.06f, .12f, .06f), new Color(.9f, .85f, .7f));
            support.localRotation = Quaternion.Euler(0, 0, 60);
        }
    }

    public static Transform Part(PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        var collider = go.GetComponent<Collider>(); if (collider != null) Object.Destroy(collider);
        go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
        var renderer = go.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader != null) renderer.material = new Material(shader);
        renderer.material.color = color;
        return go.transform;
    }

    // A placeholder person (Bachué, Custodio...): robe, head and a light so they read at distance.
    public static Transform Figure(string name, Transform parent, Vector3 position, Color robe, Color accent, float height = 1.75f)
    {
        var prefab = Resources.Load<GameObject>("Characters/" + name.Replace(" ", ""));
        var root = new GameObject(name).transform;
        root.SetParent(parent, true); root.position = position;
        if (prefab != null) { Object.Instantiate(prefab, root, false); return root; }
        float k = height / 1.75f;
        Part(PrimitiveType.Cylinder, root, new Vector3(0, .55f * k, 0), new Vector3(.55f, .55f, .45f) * k, robe);
        Part(PrimitiveType.Capsule, root, new Vector3(0, 1.15f * k, 0), new Vector3(.42f, .35f, .32f) * k, robe * 1.1f);
        Part(PrimitiveType.Sphere, root, new Vector3(0, 1.6f * k, 0), Vector3.one * .26f * k, new Color(.55f, .38f, .26f));
        Part(PrimitiveType.Cube, root, new Vector3(0, 1.38f * k, .12f), new Vector3(.36f, .06f, .05f) * k, accent);
        return root;
    }
}
