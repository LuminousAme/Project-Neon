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
        [Header("Move & Move Local Settings")]
        public bool overrideStartPosition;
        public Vector3 startPositionOverride;
        public Vector3 MoveTarget;

        [Space]
        [Header("Rotate and Rotate Local Settings")]
        public bool overrideStartRotation;
        public Vector3 startRotationOverride;
        public Vector3 targetRotation;
        [Header("Rotate Around and Rotate Around Local Settings")]
        public Vector3 aroundAxis;
        public float angle;

        [Space]
        [Header("Scale Settings")]
        public bool overrideStartScale;
        public Vector3 startScaleOverride;
        public Vector3 targetScale;

        [Space]
        [Header("UI Size Settings, Note: Requires RectTransform")]
        public bool overrideStartUISize;
        public Vector2 startUISizeOverride;
        public Vector2 targetUISize;

        [Space]
        [Header("UI Alpha Settings: Note Requires CanvasGroup")]
        public bool overrideStartAlpha;
        public float startAlphaOverride;
        public float targetAlpha;
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
                    if (effects[index].overrideStartPosition) transform.position = effects[index].startPositionOverride;
                    tween = LeanTween.move(gameObject, effects[index].MoveTarget, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.MOVELOCAL:
                    if (effects[index].overrideStartPosition) transform.localPosition = effects[index].startPositionOverride;
                    tween = LeanTween.moveLocal(gameObject, effects[index].MoveTarget, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATE:
                    if (effects[index].overrideStartRotation) transform.rotation = Quaternion.Euler(effects[index].startRotationOverride);
                    tween = LeanTween.rotate(gameObject, effects[index].targetRotation, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATELOCAL:
                    if (effects[index].overrideStartRotation) transform.localRotation = Quaternion.Euler(effects[index].startRotationOverride);
                    tween = LeanTween.rotateLocal(gameObject, effects[index].targetRotation, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATEAROUNDAXIS:
                    if (effects[index].overrideStartRotation) transform.rotation = Quaternion.Euler(effects[index].startRotationOverride);
                    tween = LeanTween.rotateAround(gameObject, effects[index].aroundAxis, effects[index].angle, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ROTATEAROUNDAXISLOCAL:
                    if (effects[index].overrideStartRotation) transform.localRotation = Quaternion.Euler(effects[index].startRotationOverride);
                    tween = LeanTween.rotateAroundLocal(gameObject, effects[index].aroundAxis, effects[index].angle, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.SCALE:
                    if (effects[index].overrideStartScale) transform.localScale = effects[index].startScaleOverride;
                    tween = LeanTween.scale(gameObject, effects[index].targetScale, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.UISIZE:
                    RectTransform trans = this.GetComponent<RectTransform>();
                    if (effects[index].overrideStartUISize) trans.SetSize(effects[index].startUISizeOverride);
                    tween = LeanTween.size(trans, effects[index].targetUISize, effects[index].duration).setDelay(effects[index].startDelay);
                    break;

                case TweenType.ALPHA:
                    CanvasGroup canvasGroup = this.GetComponent<CanvasGroup>();
                    if (effects[index].overrideStartAlpha) canvasGroup.alpha = effects[index].startAlphaOverride;
                    tween = LeanTween.alphaCanvas(canvasGroup, effects[index].targetAlpha, effects[index].duration).setDelay(effects[index].startDelay);
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