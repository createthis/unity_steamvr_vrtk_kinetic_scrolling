using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticScroller : MonoBehaviour {
    public FixedJoint fixedJoint;
    public GameObject leftEndStop;
    public GameObject rightEndStop;
    public float space = 0.1f;

    private List<GameObject> list;
    private Vector3 origin;
    private float height;
    private bool listChanged = false;
    private BoxCollider boxCollider;
    private new Rigidbody rigidbody;
    private ConfigurableJoint slidingJoint;

    private void AddFixedJoint() {
        fixedJoint = gameObject.AddComponent<FixedJoint>();
    }

    public void OnTriggerDown(Transform controller, int controllerIndex) {
        Debug.Log("OnTriggerDown");
        //ConfigurableJoint configurableJoint = GetComponent<ConfigurableJoint>();
        //configurableJoint.anchor = controller.position;
        Rigidbody controllerRigidbody = controller.gameObject.GetComponent<Rigidbody>();
        if (!controllerRigidbody) {
            controllerRigidbody = controller.gameObject.AddComponent<Rigidbody>();
            controllerRigidbody.isKinematic = true;
            controllerRigidbody.useGravity = false;
        }
        AddFixedJoint();
        fixedJoint.connectedBody = controllerRigidbody;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Debug.Log("fixedJoint=" + fixedJoint);
    }

    public void OnTriggerUpdate(Transform controller, int controllerIndex) {
        Rigidbody controllerRigidbody = controller.gameObject.GetComponent<Rigidbody>();
        SteamVR_Controller.Device device = SteamVR_Controller.Input((int)controllerIndex);
        controllerRigidbody.velocity = device.velocity;
        controllerRigidbody.angularVelocity = device.angularVelocity;
    }

    public void OnTriggerUp(Transform controller, int controllerIndex) {
        Debug.Log("OnTriggerUp");
        if (!fixedJoint) return;
        fixedJoint.connectedBody = null;
        Destroy(fixedJoint);
        SteamVR_Controller.Device device = SteamVR_Controller.Input((int)controllerIndex);
        Debug.Log("velocity=" + device.velocity + ",angularVelocity=" + device.angularVelocity);
        rigidbody.velocity = device.velocity;
        rigidbody.angularVelocity = device.angularVelocity;
    }

    public void SetHeight(float height) {
        this.height = height;
        listChanged = true;
        Initialize();
    }

    public void SetOrigin(Vector3 position) {
        origin = position;
    }

    public void SetList(List<GameObject> myList) {
        list = myList;
        listChanged = true;
        Initialize();
    }

    public void Initialize() {
        if (!listChanged) return;

        if (boxCollider == null) {
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        if (list != null) {
            ScaleGameObjects();
            ResizeCollider();
            UpdateSlidingJoint();
            UpdateEndStops();
        }
        listChanged = false;
    }

    private void UpdateEndStops() {
        float newScale = RatioCalculator(leftEndStop.transform.localScale.x, 1f, height);
        Vector3 newScaleVector3 = new Vector3(newScale, newScale, newScale);
        leftEndStop.transform.localScale = newScaleVector3;
        rightEndStop.transform.localScale = newScaleVector3;
        float width = Width();
        leftEndStop.transform.localPosition = new Vector3(-width, 0, 0);
        rightEndStop.transform.localPosition = new Vector3(width, 0, 0);
    }

    private void UpdateSlidingJoint() {
        float width = Width();
        slidingJoint.anchor = new Vector3(-width / 2, 0, 0);
        slidingJoint.connectedAnchor = new Vector3(-width / 2, 0, 0);
        /*
        SoftJointLimit limit = slidingJoint.linearLimit;
        limit.limit = width/2;
        slidingJoint.linearLimit = limit;
        */
    }

    private void ResizeCollider() {
        Vector3 size = boxCollider.size;
        size.x = list.Count * height;
        size.y = height;
        size.z = height;
        boxCollider.size = size;
        boxCollider.center = new Vector3(size.x / 2, 0, 0);
    }

    private float RatioCalculator(float a, float b, float c) {
        return c * b / a;
    }

    private float Width() {
        return list.Count * (height + space);
    }

    private void PositionGameObject(GameObject myGameObject, int index) {
        float width = Width();
        Debug.Log("setting parent for" + myGameObject.name);
        myGameObject.transform.parent = gameObject.transform;
        myGameObject.transform.localPosition = new Vector3(index * (height + space) - width/2, 0, 0);
    }

    private void ScaleGameObject(GameObject myGameObject) {
        Bounds bounds = myGameObject.GetComponent<MeshFilter>().mesh.bounds;
        float x = bounds.size.x;
        float y = bounds.size.y;
        float z = bounds.size.z;
        float max = x;
        if (y > max) max = y;
        if (z > max) max = z;

        float newScale = RatioCalculator(max, 1f, height);
        //Debug.Log("height=" + height + ",max=" + max + ",newScale=" + newScale);
        myGameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
    }

    private void ScaleGameObjects() {
        for (int i = 0; i < list.Count; i++) {
            PositionGameObject(list[i], i);
            ScaleGameObject(list[i]);
        }
    }

    // Use this for initialization
    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        slidingJoint = GetComponent<ConfigurableJoint>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update() {

    }
}
