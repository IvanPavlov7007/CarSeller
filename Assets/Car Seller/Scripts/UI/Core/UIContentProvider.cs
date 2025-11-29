using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UIContentProvider
{
    IEnumerable<UIContent> GetContents(UIContext context);
}
