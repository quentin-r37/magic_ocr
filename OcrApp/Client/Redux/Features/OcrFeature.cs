using Fluxor;
using OcrApp.Client.Redux.State;

namespace OcrApp.Client.Redux.Features;

public class OcrFeature : Feature<OcrState>
{
    public override string GetName() => "Ocr";

    protected override OcrState GetInitialState() => OcrState.Initial;
}