using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ArManager : MonoBehaviour
{
    enum ARNetworkType
    {
        Host,
        Client
    }

    private TabletNetType _CurrentArNetworkType = null;

    /// <summary>
    ///     
    /// </summary>
    public TabletManager.ARNetworkType CurrentArNetworkType
    {
        get
        {
            return this._CurrentArNetworkType;
        }

        set
        {
            // Only set if not set already
            if(this._CurrentArNetworkType == null)
            {
                this._CurrentArNetworkType = value;
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
