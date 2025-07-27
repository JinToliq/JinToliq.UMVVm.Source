using System;
using System.Collections.Generic;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding.Condition
{
  public enum Relation
  {
    And = 0,
    Or = 1,
  }

#if UNITY_EDITOR
  [UnityEditor.CustomEditor(typeof(ConditionBinding), true)]
  public class ConditionBindingEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      base.OnInspectorGUI();

      var binding = (ConditionBinding)target;

      if (!Application.isPlaying)
        return;

      UnityEditor.EditorGUILayout.Space();
      UnityEditor.EditorGUILayout.Space();
      foreach (var condition in binding.Conditions)
        UnityEditor.EditorGUILayout.LabelField(condition.ToInspectString());
    }
  }
#endif

  public abstract class ConditionBinding : BaseBinding
  {
    [SerializeField] private Relation _relation;
    [SerializeField] private ConditionBindingEntry[] _condition;

    internal IReadOnlyList<ConditionBindingEntry> Conditions => _condition;

    internal void OnEntryChanged(ConditionBindingEntry entry) =>
      EvaluateFromCached();

    protected override void OnAwakened()
    {
      base.OnAwakened();
      if (_condition is null || _condition.Length <= 0)
        return;

      foreach (var condition in _condition)
        condition.SetParent(this);
    }

    protected override void Bind()
    {
      foreach (var item in _condition)
        item.Bind();
    }

    protected override void OnBound()
    {
      base.OnBound();
      ForceEvaluate();
    }

    protected override void OnEnabled() =>
      ForceEvaluate();

    protected override void Unbind()
    {
      foreach (var item in _condition)
        item.Unbind();
    }

    protected abstract void OnEvaluated(bool result);

    private void EvaluateFromCached()
    {
      if (_condition is null || _condition.Length < 1)
        throw new("Conditions not set");

      var result = _condition[0].LastResult;
      if (_condition.Length > 1)
      {
        for (var i = 1; i < _condition.Length; i++)
        {
          result = _relation switch
          {
            Relation.And => result && _condition[i].LastResult,
            Relation.Or => result || _condition[i].LastResult,
            _ => throw new ArgumentOutOfRangeException(nameof(_relation), _relation, "Unknown relation type"),
          };
        }
      }

      OnEvaluated(result);
    }

    private void ForceEvaluate()
    {
      if (_condition is null || _condition.Length < 1)
        throw new("Conditions not set");

      var result = _condition[0].ForceEvaluate();
      if (_condition.Length > 1)
      {
        for (var i = 1; i < _condition.Length; i++)
        {
          result = _relation switch
          {
            Relation.And => result && _condition[i].ForceEvaluate(),
            Relation.Or => result || _condition[i].ForceEvaluate(),
            _ => throw new ArgumentOutOfRangeException(nameof(_relation), _relation, "Unknown relation type"),
          };
        }
      }

      OnEvaluated(result);
    }
  }
}