// Plaza Núñez camera session: the shared HandTrackingSession, with the mouse as the fallback.
public sealed class PlazaHandSession : HandTrackingSession
{
    protected override string Fallback => "pulsa M para usar el mouse";
}
