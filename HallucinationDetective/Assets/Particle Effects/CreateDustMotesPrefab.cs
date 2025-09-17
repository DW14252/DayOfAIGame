// Assets/Editor/CreateDustMotesPrefab.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateDustMotesPrefab {
    [MenuItem("Tools/Create Dust Motes Prefab")]
    public static void Create() {
        var go = new GameObject("DustMotes");
        var ps = go.AddComponent<ParticleSystem>();
        var pr = go.GetComponent<ParticleSystemRenderer>();

        // MAIN
        var main = ps.main;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 15f);
        main.startSpeed   = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startSize    = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startColor   = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.25f));
        main.maxParticles = 2000;

        // EMISSION
        var emission = ps.emission;
        emission.rateOverTime = 12f;

        // SHAPE (wide, thin box in front of camera)
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(30f, 15f, 1f);

        // COLOR OVER LIFETIME (fade in/out)
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.3f, 0.15f),
                new GradientAlphaKey(0.3f, 0.85f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);

        // SIZE OVER LIFETIME (slight swell)
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.8f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0.9f)
        );
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // NOISE (gentle drift)
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.2f;
        noise.scrollSpeed = 0.02f;

        // RENDERER
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingFudge = -5f; // bias behind UI
        // Built-in legacy alpha-blend shader works everywhere
        var mat = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
        var tex = Resources.GetBuiltinResource<Texture2D>("Default-Particle.png");
        mat.mainTexture = tex;
        pr.sharedMaterial = mat;

        // Save prefab
        System.IO.Directory.CreateDirectory("Assets");
        string path = "Assets/DustMotes.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path, out bool success);
        Object.DestroyImmediate(go);
        if (success) {
            Debug.Log("Created: " + path);
            AssetDatabase.Refresh();
        } else {
            Debug.LogError("Failed to create prefab.");
        }
    }
}
#endif
