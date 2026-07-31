using UnityEngine;

public static class ConfettiBurst
{
    private static Material material;

    public static void Play(Vector3 position, int count = 40)
    {
        GameObject host = new GameObject("ConfettiBurst");
        host.transform.position = position;

        ParticleSystem system = host.AddComponent<ParticleSystem>();
        system.Stop();

        ParticleSystem.MainModule main = system.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.5f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 1.8f;
        main.startColor = Palette();
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.startSize3D = true;
        main.startSizeX = new ParticleSystem.MinMaxCurve(0.14f, 0.22f);
        main.startSizeY = new ParticleSystem.MinMaxCurve(0.06f, 0.11f);

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = system.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f;
        shape.radiusThickness = 1f;
        shape.arc = 360f;

        ParticleSystem.RotationOverLifetimeModule spin = system.rotationOverLifetime;
        spin.enabled = true;
        spin.separateAxes = false;
        spin.z = new ParticleSystem.MinMaxCurve(-8f, 8f);

        ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
        fade.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.65f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        ParticleSystemRenderer renderer = host.GetComponent<ParticleSystemRenderer>();
        renderer.material = ConfettiMaterial();
        renderer.sortingOrder = 500;

        system.Play();
    }

    private static ParticleSystem.MinMaxGradient Palette()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.78f, 0.24f), 0.00f),
                new GradientColorKey(new Color(0.95f, 0.42f, 0.35f), 0.25f),
                new GradientColorKey(new Color(0.35f, 0.72f, 0.95f), 0.50f),
                new GradientColorKey(new Color(0.34f, 0.82f, 0.55f), 0.75f),
                new GradientColorKey(new Color(0.72f, 0.55f, 0.95f), 1.00f),
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });

        return new ParticleSystem.MinMaxGradient(gradient)
        {
            mode = ParticleSystemGradientMode.RandomColor
        };
    }

    private static Material ConfettiMaterial()
    {
        if (material != null)
            return material;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        return material;
    }
}
