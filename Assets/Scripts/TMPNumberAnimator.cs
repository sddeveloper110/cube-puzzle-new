using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TMPNumberAnimator : MonoBehaviour
{
    public void Animate(TextMeshProUGUI text, float totalDuration, float scaleUp = 1.8f)
    {
        text.ForceMeshUpdate();

        List<List<int>> numberGroups = GetNumberGroups(text.text);
        float stepDuration = totalDuration / numberGroups.Count;

        Sequence sequence = DOTween.Sequence();
       
        foreach (var group in numberGroups)
        {
            sequence.Append(
                DOTween.To(
                    () => 0f,
                    v => ScaleCharacters(text, group, Mathf.Lerp(1f, scaleUp, v)),
                    1f,
                    stepDuration * 0.5f
                )
            );

            sequence.Append(
                DOTween.To(
                    () => 0f,
                    v => ScaleCharacters(text, group, Mathf.Lerp(scaleUp, 1f, v)),
                    1f,
                    stepDuration * 0.5f
                )
            );
        }
    }

    // ---------------- HELPERS ----------------

    private List<List<int>> GetNumberGroups(string input)
    {
        List<List<int>> groups = new List<List<int>>();
        List<int> currentGroup = null;

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsDigit(input[i]))
            {
                if (currentGroup == null)
                {
                    currentGroup = new List<int>();
                    groups.Add(currentGroup);
                }
                currentGroup.Add(i);
            }
            else
            {
                currentGroup = null;
            }
        }

        return groups;
    }

    private void ScaleCharacters(TextMeshProUGUI text, List<int> charIndices, float scale)
    {
        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;

        Vector3 center = Vector3.zero;
        int visibleCount = 0;

        foreach (int charIndex in charIndices)
        {
            if (!textInfo.characterInfo[charIndex].isVisible)
                continue;

            int matIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[charIndex].vertexIndex;

            Vector3[] verts = textInfo.meshInfo[matIndex].vertices;
            center += (verts[vertIndex] + verts[vertIndex + 2]) * 0.5f;
            visibleCount++;
        }

        if (visibleCount == 0)
            return;

        center /= visibleCount;

        foreach (int charIndex in charIndices)
        {
            if (!textInfo.characterInfo[charIndex].isVisible)
                continue;

            int matIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[charIndex].vertexIndex;

            Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

            for (int i = 0; i < 4; i++)
            {
                verts[vertIndex + i] =
                    (verts[vertIndex + i] - center) * scale + center;
            }
        }

        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
