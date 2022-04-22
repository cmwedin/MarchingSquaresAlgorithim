using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class MarchingSquares : MonoBehaviour
{
    //* editor values
    [SerializeField] private float threshold;
    [SerializeField] private float resolution;
    
    //* private values
    private Func<Vector2,float> Potential;
    private bool running = false;
    
    //*public methods
    public void SetPotential(Func<Vector2,float> potential) {
        Potential = potential;
    }
    //? public wrapper to ensure the algorithm isnt called multiple times per instance 
    //? probably a more elegant way to do this
    public void Run() {
        if(running) {
            Debug.LogWarning("Marching squares algorthim called while already running");
            return;
        }
        run();
        return;
    }

    //*private methods
    //? Main
    private void run() {
        var watch = Stopwatch.StartNew(); //? for performance testing
        running = true;

        running = false;
        watch.Stop();
        Debug.Log($"algorithm run in {watch.ElapsedMilliseconds}ms");
        return;
    }

    //* Monobehavior methods
    //? Start is called before the first frame update
    void Start()
    {
        if(Potential == null) {
            Debug.LogWarning("Potential not set, using default magnitude potential");
            SetPotential(x => x.magnitude);
        }

    }

    //? Update is called once per frame
    void Update()
    {
        
    }
}
