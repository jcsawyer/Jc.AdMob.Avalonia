using System.Collections.Generic;
using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Jc.AdMob.Avalonia;

internal sealed class AdMobAutomationPeer(Control owner) : ControlAutomationPeer(owner)
{
    protected override IReadOnlyList<AutomationPeer> GetOrCreateChildrenCore()
        => [];

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Custom;
}