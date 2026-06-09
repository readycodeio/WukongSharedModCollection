using CommB1;
using System;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using UnrealEngine.Slate;
using UnrealEngine.UMG;
using WukongMp.Api;
using WukongMp.Api.UI;
using WukongMp.Api.WukongUtils;

public static class UI
{
    public static UUserWidget mainHUD;
    public static UCanvasPanel root;

    public static UCanvasPanelSlot seekerScoreSlot;
    public static UCanvasPanelSlot hiderScoreSlot;
    public static UCanvasPanelSlot TimerSlot;

    public static URichTextBlock seekerScoreText;
    public static URichTextBlock hiderScoreText;
    public static URichTextBlock TimerText;

    public static void Initialize()
    {
        try
        {
            mainHUD = new UUserWidget();

            root = new UCanvasPanel();
            root.AddToRoot();

            TimerText = new URichTextBlock();
            TimerText.SetText(FText.FromString("00:00.00"));

            seekerScoreText = new URichTextBlock();
            seekerScoreText.SetText(FText.FromString("Seekers: 0"));

            hiderScoreText = new URichTextBlock();
            hiderScoreText.SetText(FText.FromString("0 :Hiders"));

            TimerSlot = root.AddChildToCanvas(TimerText);
            seekerScoreSlot = root.AddChildToCanvas(seekerScoreText);
            hiderScoreSlot = root.AddChildToCanvas(hiderScoreText);

            FAnchors bottomCenterAnchor = new();
            bottomCenterAnchor.Minimum = new FVector2D(0.5f, 1f);
            bottomCenterAnchor.Maximum = new FVector2D(0.5f, 1f);

            TimerSlot.SetAnchors(bottomCenterAnchor);
            TimerSlot.SetAlignment(new FVector2D(0.5f, 1f));
            TimerSlot.SetPosition(new FVector2D(0, -50));
            TimerSlot.SetAutoSize(true);

            seekerScoreSlot.SetAnchors(bottomCenterAnchor);
            seekerScoreSlot.SetAlignment(new FVector2D(1f, 1f));
            seekerScoreSlot.SetPosition(new FVector2D(-200, -50));
            seekerScoreSlot.SetAutoSize(true);

            hiderScoreSlot.SetAnchors(bottomCenterAnchor);
            hiderScoreSlot.SetAlignment(new FVector2D(0f, 1f));
            hiderScoreSlot.SetPosition(new FVector2D(200, -50));
            hiderScoreSlot.SetAutoSize(true);

            UUserWidget userwidget = new();
            userwidget.GetParent().AddChild(root);
            userwidget.AddToViewport(999);

            var pc = GameUtils.GetPlayerController();
            pc.GetHUD().DrawText("AVC", FLinearColor.White, -50, -50, null, 1f, false);
        }
        catch (Exception ex)
        {
            // Logging.LogError(ex.Message);
        }
    }
}