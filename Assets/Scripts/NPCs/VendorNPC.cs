using System.Collections.Generic;
using UnityEngine;

public class VendorNPC : MonoBehaviour
{
    [Header("Vendor")]
    [SerializeField] private string vendorName = "Merchant";
    [SerializeField] private List<ItemData> itemsForSale = new List<ItemData>();

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private KeyCode interactionKey = KeyCode.F;

    public string VendorName => vendorName;
    public IReadOnlyList<ItemData> ItemsForSale => itemsForSale;
    public float InteractionRange => interactionRange;
    public Transform Player => player;

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || VendorUI.Instance == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionRange && Input.GetKeyDown(interactionKey))
        {
            VendorUI.Instance.OpenVendor(this);
        }

        if (VendorUI.Instance.IsOpenFor(this) && distance > interactionRange)
        {
            VendorUI.Instance.CloseVendor();
        }
    }
}