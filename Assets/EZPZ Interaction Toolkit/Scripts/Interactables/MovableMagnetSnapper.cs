//EZPZ Interaction Toolkit
//by Matt Cabanag
//created 28 Nov Jun 2023

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovableMagnetSnapper : MonoBehaviour
{
    [Header("Snap Settings")]
    public Transform snappingPoint;
    public bool alignRotation = true;
    [Tooltip("Leave empty to accept all items")]
    public string filterString;

    [Header("Event Handling")]
    public UnityEvent onSnap;
    public UnityEvent onRelease;
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;
    public UnityEvent onTriggerStay;


    [Header("System Stuff (Usually Don't Touch)")]
    public Movable subject;
    public bool snapFlag = true;    
    public Vector3 subjectLocalAttachPos;

    // Start is called before the first frame update
    void Start()
    {
        if (snappingPoint == null)
            snappingPoint = transform;

        snapFlag = true;

        if(snappingPoint.parent.localScale != Vector3.one || snappingPoint.localScale != Vector3.one)
        {
            Debug.LogError("SNAPPING POINT SCALE MISMATCH " + name + " snapping point scale or its parent is not (1,1,1)");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(subject != null)
        {
            HandleSnapping();
            HandleFixedPos();
        }
    }

    public void HandleSnapping()
    {
        if (!snapFlag)
        {
            if (!subject.moving && subject.myMagnetSnapper == null)
            {
                Vector3 attachPos = snappingPoint.position;
                subject.myMagnetSnapper = this;

                if (subject.attachPoint == null)
                {
                    subject.transform.parent = snappingPoint;
                    subject.transform.localPosition = Vector3.zero;
                    subject.transform.rotation = snappingPoint.rotation;
                }
                else
                {
                    //swap parentage first
                    subject.attachPoint.parent = null;
                    subject.transform.parent = subject.attachPoint;

                    //align rotations and positions;
                    subject.attachPoint.transform.position = attachPos;
                    subject.attachPoint.transform.rotation = snappingPoint.rotation;

                    //return parentage
                    subject.transform.parent = snappingPoint;
                    subject.attachPoint.parent = snappingPoint.transform;
                    subjectLocalAttachPos = subject.transform.localPosition;
                }

                Debug.Log("On Snap!");
                onSnap.Invoke();

                if (subject.myRbody != null)
                {
                    subject.myRbody.linearVelocity = Vector3.zero;
                    subject.myRbody.useGravity = false;
                    //r.isKinematic = true;
                }

                snapFlag = true;
            }
        }        
    }

    public void HandleFixedPos()
    {
        if (!subject.moving)
        {
            if (subject.transform.parent == snappingPoint)
            {
                if (snapFlag)
                {
                    if (subject.myRbody != null)
                        subject.myRbody.linearVelocity = Vector3.zero;

                    subject.transform.localPosition = subjectLocalAttachPos;
                    subject.transform.rotation = snappingPoint.rotation;
                }
            }
            else
            {
                subject = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (subject == null)
        {
            subject = other.GetComponent<Movable>();

            if (subject != null)
            {
                if (subject.myMagnetSnapper == null)
                { 
                    if (filterString.Length > 0)
                    {
                        TriggerFilter tf = subject.GetComponent<TriggerFilter>();

                        if (tf != null)
                        {
                            if (!tf.filterString.Equals(filterString))
                            {
                                subject = null;
                                return;
                            }
                        }
                        else
                        {
                            subject = null;
                            return;
                        }

                    }

                    if (subject.myMagnetSnapper == null)
                    {
                        snapFlag = false;
                    }
                    else
                    {
                        subject = null;
                        return;
                    }
                }
            }
        }

        onTriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (subject != null)
        {
            Movable om = other.GetComponent<Movable>();

            if (om != null)
            {
                //make sure the object exiting the area
                //is exactly the one that's leaving...
                if (om == subject)
                {
                    //if (subject.moving)
                    {
                        //ReleaseSubject();
                    }
                }
                //don't want another object to trigger dropping
            }
        }
        else
        {
            onTriggerExit.Invoke();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke();
    }

    public void ReleaseSubject()
    {
        if(subject != null)
        {
            Debug.Log("ReleaseSubject");
            Rigidbody r = subject.GetComponent<Rigidbody>();           

            if (r != null)
            {
                r.useGravity = true;
                r.linearVelocity = Vector3.zero;                
                r.isKinematic = false;
            }

            onRelease.Invoke();

            subjectLocalAttachPos = Vector3.zero;
            subject.myMagnetSnapper = null;
            snapFlag = true;
            subject = null;
        }
    }
}
