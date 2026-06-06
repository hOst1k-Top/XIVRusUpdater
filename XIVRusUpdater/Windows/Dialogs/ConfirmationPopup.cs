using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace XIVRusUpdater.Windows.Dialogs;

public sealed class ConfirmationPopup
{
    private readonly string popupId;

    public bool IsOpen { get; private set; }

    public Action? OnConfirm;
    public Action? OnCancel;

    public ConfirmationPopup(string popupId)
    {
        this.popupId = popupId;
    }

    public void Open()
    {
        IsOpen = true;
        ImGui.OpenPopup(popupId);
    }

    public void Draw()
    {
        if (!ImGui.BeginPopupModal(popupId))
            return;

        ImGui.Text("Are you sure?");

        if (ImGui.Button("Confirm"))
        {
            OnConfirm?.Invoke();
            ImGui.CloseCurrentPopup();
            IsOpen = false;
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel"))
        {
            OnCancel?.Invoke();
            ImGui.CloseCurrentPopup();
            IsOpen = false;
        }

        ImGui.EndPopup();
    }
}
