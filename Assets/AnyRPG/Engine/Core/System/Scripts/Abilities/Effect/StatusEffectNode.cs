using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {
    public class StatusEffectNode {

        private StatusEffect statusEffect = null;

        private Coroutine monitorCoroutine = null;

        private AbilityEffectContext abilityEffectContext = null;

        // the reference to the character stats this node sits on
        private CharacterStats characterStats = null;

        // track state
        private int currentStacks = 1;
        private float remainingDuration = 0f;

        // list of status effect nodes to send updates to so multiple effects panels and bars can access this
        private List<StatusEffectNodeScript> statusEffectNodeScripts = new List<StatusEffectNodeScript>();

        // keep track of any spell effect prefabs associated with this status effect.
        private Dictionary<PrefabProfile, GameObject> prefabObjects = new Dictionary<PrefabProfile, GameObject>();

        public StatusEffect StatusEffect { get => statusEffect; set => statusEffect = value; }
        public Coroutine MyMonitorCoroutine { get => monitorCoroutine; set => monitorCoroutine = value; }
        public AbilityEffectContext AbilityEffectContext { get => abilityEffectContext; set => abilityEffectContext = value; }
        public Dictionary<PrefabProfile, GameObject> PrefabObjects { get => prefabObjects; set => prefabObjects = value; }
        public int CurrentStacks { get => currentStacks; set => currentStacks = value; }
        public float RemainingDuration { get => remainingDuration; set => remainingDuration = value; }

        //public void Setup(CharacterStats characterStats, StatusEffect _statusEffect, Coroutine newCoroutine) {
        public void Setup(CharacterStats characterStats, StatusEffect statusEffect, AbilityEffectContext abilityEffectContext) {
            //Debug.Log("StatusEffectNode.Setup(): " + _statusEffect.MyName);
            this.characterStats = characterStats;
            this.statusEffect = statusEffect;
            this.abilityEffectContext = abilityEffectContext;
            //this.monitorCoroutine = newCoroutine;
        }

        public void CancelStatusEffect() {
            //Debug.Log("StatusEffectNode.CancelStatusEffect(): " + StatusEffect.DisplayName);
            if (prefabObjects != null) {
                foreach (GameObject go in prefabObjects.Values) {
                    //Debug.Log(MyName + ".LengthEffect.CancelEffect(" + targetCharacter.MyName + "): Destroy: " + go.name);
                    GameObject.Destroy(go, StatusEffect.PrefabDestroyDelay);
                }
            }
            StatusEffect.CancelEffect(characterStats.BaseCharacter);
            characterStats.HandleStatusEffectRemoval(statusEffect);
            ClearNodeScripts();
        }

        public void ClearNodeScripts() {
            foreach (StatusEffectNodeScript statusEffectNodeScript in statusEffectNodeScripts) {
                if (statusEffectNodeScript != null) {
                    //Debug.Log("AbilityEffect.OnDestroy() statusEffectNodeScript is not null. destroying gameobject");
                    GameObject.Destroy(statusEffectNodeScript.gameObject);
                } else {
                    //Debug.Log("AbilityEffect.OnDestroy() statusEffectNodeScript is null!");
                }
            }
            statusEffectNodeScripts.Clear();
        }

        public void SetStatusNode(StatusEffectNodeScript statusEffectNodeScript) {
            //Debug.Log("StatusEffect.SetStatusNode()");
            statusEffectNodeScripts.Add(statusEffectNodeScript);
            UpdateStatusNode();
        }

        public void UpdateStatusNode() {
            //Debug.Log(GetInstanceID() + ".StatusEffect.UpdateStatusNode(): COUNT statusEffectNodeScript: " + statusEffectNodeScripts.Count);
            foreach (StatusEffectNodeScript statusEffectNodeScript in statusEffectNodeScripts) {
                //Debug.Log("StatusEffect.UpdateStatusNode(): got statuseffectnodescript");
                if (statusEffectNodeScript != null) {
                    string statusText = string.Empty;
                    string stackText = string.Empty;
                    if (currentStacks > 1) {
                        stackText = currentStacks.ToString();
                    }
                    if (statusEffect.LimitedDuration == true && statusEffect.ClassTrait == false) {
                        //Debug.Log(GetInstanceID() + MyName + ".StatusEffect.UpdateStatusNode(): limted");
                        float printedDuration = (int)remainingDuration;
                        if (printedDuration < 60 && printedDuration >= 0) {
                            // less than 1 minute
                            statusText = ((int)printedDuration).ToString() + "s";
                        } else if (printedDuration < 3600) {
                            //less than 1 hour
                            statusText = ((int)(printedDuration / 60)).ToString() + "m";
                        } else if (printedDuration > 3600f) {
                            //greater than 1 hour
                            statusText = ((int)(printedDuration / 3600)).ToString() + "h";
                        }
                    }

                    // set updated values
                    if (statusEffectNodeScript.MyUseTimerText == true && statusText != string.Empty) {
                        if (statusEffectNodeScript.MyTimer != null) {
                            if (statusEffectNodeScript.MyTimer.isActiveAndEnabled == false) {
                                statusEffectNodeScript.MyTimer.gameObject.SetActive(true);
                            }
                            statusEffectNodeScript.MyTimer.text = statusText;
                        }
                    } else {
                        if (statusEffectNodeScript.MyTimer != null) {
                            statusEffectNodeScript.MyTimer.gameObject.SetActive(false);
                        }
                    }
                    if (statusEffectNodeScript.MyUseStackText == true) {
                        if (statusEffectNodeScript.MyStackCount.isActiveAndEnabled == false) {
                            statusEffectNodeScript.MyStackCount.gameObject.SetActive(true);
                        }
                        statusEffectNodeScript.MyStackCount.gameObject.SetActive(true);
                        statusEffectNodeScript.MyStackCount.text = stackText;
                    } else {
                        if (statusEffectNodeScript.MyStackCount != null) {
                            statusEffectNodeScript.MyStackCount.gameObject.SetActive(false);
                        }
                    }
                    if (statusEffect.Duration == 0f) {
                        statusEffectNodeScript.UpdateFillIcon(0f);
                    } else {
                        float usedFillAmount = remainingDuration / statusEffect.Duration;
                        statusEffectNodeScript.UpdateFillIcon(usedFillAmount);
                    }
                }
            }
        }

        public bool AddStack() {
            bool returnValue = false;
            if (currentStacks < statusEffect.MaxStacks) {
                currentStacks++;
                // refresh the duration
                returnValue = true;
            }
            if (statusEffect.RefreshableDuration) {
                SetRemainingDuration(statusEffect.Duration);
            }
            return returnValue;
        }

        public void SetRemainingDuration(float remainingDuration) {
            this.remainingDuration = remainingDuration;
        }

        public float GetRemainingDuration() {
            return remainingDuration;
        }


    }
}
