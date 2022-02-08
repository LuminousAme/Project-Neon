using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class LeanTweenHelper : MonoBehaviour
{
    [Serializable]
    public enum TweenType
    {
        MOVE,
        MOVELOCAL,
        ROTATE,
        ROTATELOCAL,
        ROTATEAROUNDAXIS,
        ROTATEAROUNDAXISLOCAL,
        SCALE,
        UISIZE,
        ALPHA
    }

    [Serializable]
    public enum EaseType
    {
        NONE,
        BACK,
        BOUNCE,
        CIRC,
        CUBIC,
        ELASIC,
        EXPO
    }

    [Serializable]
    public struct TweenEffect
    {
        [Header("Basic Settings")]
        public TweenType type;
        public EaseType easeIn;
        public EaseType easeOut;
        public float duration;
        public float startDelay;
        public bool beginOnStart;

        [Space]
        public bool overrideStartValue;

        [Space]
        public Vector3 startOverrideVector;
        public Vector3 endVector;
        public Vector3 aroundaxis;
        public float angle;

        [Space]
        public Vector2 startOverrideVector2;
        public Vector2 endVector2;

        [Space]
        public float startOverrideFloat;
        public float endFloat;
    }


    public List<TweenEffect> effects;

    private void Start()
    {
        for (int i = 0; i < effects.Count; i++) if (effects[i].beginOnStart) BeginTween(i);
    }

    public void BeginTween(int index)
    {
        if(index < effects.Count)
        {
            LTDescr tween = null;

            switch (effects[index].type)
            {
                case TweenType.MOVE:
                    if (effects[index].overrideStartValue) transform.position = effects[index].startOverrideVector;
                    tween = LeanTween.move(gameObject, effects[index].endVector, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.MOVELOCAL:
                    if (effects[index].overrideStartValue) transform.localPosition = effects[index].startOverrideVector;
                    tween = LeanTween.moveLocal(gameObject, effects[index].endVector, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATE:
                    if (effects[index].overrideStartValue) transform.rotation = Quaternion.Euler(effects[index].startOverrideVector);
                    tween = LeanTween.rotate(gameObject, effects[index].endVector, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATELOCAL:
                    if (effects[index].overrideStartValue) transform.localRotation = Quaternion.Euler(effects[index].startOverrideVector);
                    tween = LeanTween.rotateLocal(gameObject, effects[index].endVector, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATEAROUNDAXIS:
                    if (effects[index].overrideStartValue) transform.rotation = Quaternion.Euler(effects[index].startOverrideVector);
                    tween = LeanTween.rotateAround(gameObject, effects[index].aroundaxis, effects[index].angle, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATEAROUNDAXISLOCAL:
                    if (effects[index].overrideStartValue) transform.localRotation = Quaternion.Euler(effects[index].startOverrideVector);
                    tween = LeanTween.rotateAroundLocal(gameObject, effects[index].aroundaxis, effects[index].angle, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.SCALE:
                    if (effects[index].overrideStartValue) transform.localScale = effects[index].startOverrideVector;
                    tween = LeanTween.scale(gameObject, effects[index].endVector, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.UISIZE:
                    RectTransform trans = this.GetComponent<RectTransform>();
                    if (effects[index].overrideStartValue) trans.SetSize(effects[index].startOverrideVector2);
                    tween = LeanTween.size(trans, effects[index].endVector2, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ALPHA:
                    CanvasGroup canvasGroup = this.GetComponent<CanvasGroup>();
                    if (effects[index].overrideStartValue) canvasGroup.alpha = effects[index].startOverrideFloat;
                    tween = LeanTween.alphaCanvas(canvasGroup, effects[index].endFloat, effects[index].duration).setDelay(effects[index].startDelay);
                    break;
            }

            if (tween != null && effects[index].easeIn != EaseType.NONE)
            {
                switch(effects[index].easeIn)
                {
                    case EaseType.BACK:
                        tween = tween.setEaseInBack();
                        break;
                    case EaseType.BOUNCE:
                        tween = tween.setEaseInBounce();
                        break;
                    case EaseType.CIRC:
                        tween = tween.setEaseInCirc();
                        break;
                    case EaseType.CUBIC:
                        tween = tween.setEaseInCubic();
                        break;
                    case EaseType.ELASIC:
                        tween = tween.setEaseInElastic();
                        break;
                    case EaseType.EXPO:
                        tween = tween.setEaseInExpo();
                        break;
                }
            }
            if (tween != null && effects[index].easeOut != EaseType.NONE)
            {
                switch (effects[index].easeOut)
                {
                    case EaseType.BACK:
                        tween = tween.setEaseOutBack();
                        break;
                    case EaseType.BOUNCE:
                        tween = tween.setEaseOutBounce();
                        break;
                    case EaseType.CIRC:
                        tween = tween.setEaseOutCirc();
                        break;
                    case EaseType.CUBIC:
                        tween = tween.setEaseOutCubic();
                        break;
                    case EaseType.ELASIC:
                        tween = tween.setEaseOutElastic();
                        break;
                    case EaseType.EXPO:
                        tween = tween.setEaseOutExpo();
                        break;
                }
            }
        }
      
    }

    public void BeginAll()
    {
        for(int i = 0; i < effects.Count; i++) BeginTween(i);
    }
}