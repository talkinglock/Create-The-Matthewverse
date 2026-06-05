using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using GNodeArray = Godot.Collections.Array<Godot.Node>;
public partial class Standard
{
    public async Task Wait(float timeSec)
    {
        await Task.Delay((int)(timeSec * 1000));
    }
    public Node GetRandomChildFromParent(Node parent)
    {
        GNodeArray children = parent.GetChildren();
        int childrenCount = parent.GetChildCount();
        if (childrenCount == 0)
        {
            GD.PrintErr("Cannot select random child if there is no children!");
            return null;
        }
        int iterator = 0;
        int randomChild = (Math.Abs((int)GD.Randi()) % childrenCount)+1;
        foreach (Node child in children)
        {
            iterator++;
            if (iterator == randomChild)
            {
                return child;
            }
        }
        return null;
    }
}