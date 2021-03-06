﻿using UnityEngine;
using System.Collections;
using System;
using FlyingEggUtils;
using System.Collections.Generic;
using Difficulty;

public class ComboGenerator : GameComponent
{

    enum ComboType
    {
        DIAGONAL,
        CURVED,
        STRAIGHT,
    }

    public GameObject QTEPrefab;
    public GameObject QTESpawn;

    public Sprite a, b, x, y;
    public Sprite mobile_a, mobile_b, mobile_x, mobile_y;

    public float minHorizontalDis = 1.3f;

    private System.Random rnd;
    private QuickTimeEventController lastQTE;
    private bool comboInProgress = false;
    private float DistanceUntilNextCombo = 0f;

    private bool firstTime = true;

    private VerticalBounds bounds;
    
	// Use this for initialization
	void Start () {
        rnd = new System.Random();
        bounds = GetBounds();
    }

    void Update()
    {       
        if (isActive)
        {
            //Check if lastQTE is null because of a bug and then find the last spawned QTE --- THIS IS A QUICKFIX
            if (lastQTE == null && !firstTime)
            {
                Vector3 pos = new Vector3(-99, 0, 0);
                foreach (GameObject qte in GameObject.FindGameObjectsWithTag("QTE"))
                {
                    Vector3 qtePosition = qte.transform.position;
                    if (qtePosition.x > pos.x)
                    {
                        pos.x = qtePosition.x;
                        lastQTE = qte.GetComponent<QuickTimeEventController>();
                    }                    
                }
            }

            //Spawn new combo with random combo type
            if (CanSpawnNewCombo())
            {
                StartCombo((ComboType)rnd.Next(0, Enum.GetNames(typeof(ComboType)).Length));
            }
        }        
    }    

    private void StartCombo(ComboType type)
    {
        //Set comboinprogress so multiple combo's can't be spawned at once
        comboInProgress = true;

        float distance = minHorizontalDis;
        List<QuickTimeEvent.Type> qteTypes = new List<QuickTimeEvent.Type>();
        Vector3 start = QTESpawn.transform.position;
        float ComboLength = rnd.Next(3, 8);
        switch (DifficultyManager.Instance.GetDifficulty())
        {
            case DifficultyLevel.VERYEASY:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1.2f;
                DistanceUntilNextCombo = distance * 1.5f;
                break;
            case DifficultyLevel.EASY:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1.2f;
                DistanceUntilNextCombo = distance * 1.5f;
                break;
            case DifficultyLevel.NORMAL:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1.1f;
                DistanceUntilNextCombo = distance * 1.4f;
                break;
            case DifficultyLevel.HARD:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1.05f;
                DistanceUntilNextCombo = distance * 1.2f;
                break;
            case DifficultyLevel.VERYHARD:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1.05f;
                DistanceUntilNextCombo = distance * 1.2f;
                break;
            case DifficultyLevel.INSANE:
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                qteTypes.Add(QuickTimeEvent.Utils.GetRandomType(rnd, qteTypes));
                distance *= 1f;
                DistanceUntilNextCombo = distance * 1.1f;
                break;            
        }

        Vector3 pos;
        if (!lastQTE)
        {
            pos = start;
            pos.y = rnd.Next((int)bounds.bottom, (int)bounds.top);
        }
        else
        {
            pos = lastQTE.transform.position;
            //pos.y = rnd.Next((int)bounds.bottom, (int)bounds.top);
            pos.x += DistanceUntilNextCombo;
        }
        int typeCounter = 0;

        //OPTIONAL VARIABLE FOR CURVED COMBO ONLY
        float curveMultiplier = UnityEngine.Random.Range(8f, 10f);
        //OPTIONAL VARIABLE FOR DIAGONAL COMBO ONLY
        float upwardDistance = distance / 4f;
        upwardDistance = UnityEngine.Random.Range(upwardDistance, upwardDistance + 2f);
        float dDistance = distance;

        
        for (int i = 0; i < ComboLength; i++)
        {
            GameObject spawn = Instantiate(QTEPrefab);
            Vector3 spawnPosition = pos;
            //DO TYPE SPECIFIC POSITIONING HERE
            //type = ComboType.STRAIGHT;
            switch (type)
            {
                case ComboType.STRAIGHT:
                    
                    spawnPosition.x += distance * i;
                    spawn.transform.position = spawnPosition;
                    break;
                case ComboType.CURVED:
                    if (spawnPosition.y >= 0 && i == 0)
                        curveMultiplier *= -1;
                    spawnPosition.y += (i * i) / curveMultiplier;
                    spawnPosition.x += (distance * i) / 1;
                    spawn.transform.position = spawnPosition;
                    break;
                case ComboType.DIAGONAL:
                    if (spawnPosition.y >= 0 && i == 0)
                        upwardDistance *= -1;
                    spawnPosition.x += dDistance * i;
                    spawnPosition.y += upwardDistance * i;
                    spawn.transform.position = spawnPosition;
                    break;
            }            

            if (spawn.transform.position.y > bounds.top || spawn.transform.position.y < bounds.bottom)
            {
                Destroy(spawn);
                continue;
            }

            QuickTimeEventController qteC = spawn.GetComponent<QuickTimeEventController>();
            
            switch (DifficultyManager.Instance.GetDifficulty())
            {
                case DifficultyLevel.VERYEASY:
                    qteC.type = qteTypes[typeCounter];
                    if (typeCounter + 2 > qteTypes.Count)
                        typeCounter = 0;
                    else
                        typeCounter++;
                    break;
                case DifficultyLevel.EASY:
                    qteC.type = qteTypes[rnd.Next(0, qteTypes.Count)];
                    break;
                case DifficultyLevel.NORMAL:
                    qteC.type = qteTypes[typeCounter];
                    if (typeCounter + 2 > qteTypes.Count)
                        typeCounter = 0;
                    else
                        typeCounter++;
                    break;
                case DifficultyLevel.HARD:
                    qteC.type = qteTypes[rnd.Next(0, qteTypes.Count)];
                    break;
                case DifficultyLevel.VERYHARD:
                    qteC.type = qteTypes[rnd.Next(0, qteTypes.Count)];
                    break;
                case DifficultyLevel.INSANE:
                    qteC.type = qteTypes[rnd.Next(0, qteTypes.Count)];
                    break;
            }

            SpriteRenderer[] sprites = spawn.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sr in sprites)
            {
                switch (qteC.type)
                {
                    case QuickTimeEvent.Type.A:
                        sr.sprite = a;
#if UNITY_ANDROID
                        sr.sprite = mobile_a;
#endif
                        break;
                    case QuickTimeEvent.Type.B:
                        sr.sprite = b;
#if UNITY_ANDROID
                        sr.sprite = mobile_b;
#endif
                        break;
                    case QuickTimeEvent.Type.Y:
                        sr.sprite = y;
#if UNITY_ANDROID
                        sr.sprite = mobile_y;
#endif
                        break;
                    case QuickTimeEvent.Type.X:
                        sr.sprite = x;
#if UNITY_ANDROID
                        sr.sprite = mobile_x;
#endif
                        break;
                }
            }

            lastQTE = qteC;
        //END OF QTE INSTANTIATOR LOOP            
        }

        comboInProgress = false;
    }

    private bool CanSpawnNewCombo()
    {
        bool result = false;
        if (firstTime)
        {
            firstTime = false;
            result = true;
            return result;
        }

        if (!comboInProgress)
        {
            //result = ((enviromentPos.x - lastEnviromentPos.x) > minHorizontalDis);
            result = ((lastQTE.transform.position.x + DistanceUntilNextCombo) < QTESpawn.transform.position.x);
        }

        return result;
    }

    public void StartGenerator(bool activate)
    {
        isActive = activate;
        firstTime = true;
    }

    public VerticalBounds GetBounds()
    {
        Vector3[] corners = new Vector3[4];
        Camera camera = Camera.main;
        GetCorners(camera, 0, ref corners);

        float distance_y = Vector3.Distance(corners[0], corners[2]);

        float distance = distance_y / 2;

        VerticalBounds bounds = new VerticalBounds();
        bounds.bottom = Camera.main.transform.position.y - distance + 1f;
        bounds.top = Camera.main.transform.position.y + distance - 1f;

        return bounds;
    }

    public static void GetCorners(Camera camera, float distance, ref Vector3[] corners)
    {
        Array.Resize(ref corners, 4);

        // Top left
        corners[0] = camera.ViewportToWorldPoint(new Vector3(0, 1, distance));

        // Top right
        corners[1] = camera.ViewportToWorldPoint(new Vector3(1, 1, distance));

        // Bottom left
        corners[2] = camera.ViewportToWorldPoint(new Vector3(0, 0, distance));

        // Bottom right
        corners[3] = camera.ViewportToWorldPoint(new Vector3(1, 0, distance));
    }
}
