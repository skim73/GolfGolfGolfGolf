using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWorldController : MonoBehaviour
{
    public int current_level = 0;
    public GameObject[] LEVELS;
    public MainGameController MAIN_GAME_CONTROLLER;
    // Start is called before the first frame update
    void Start()
    {
        MAIN_GAME_CONTROLLER = GameObject.Find("Main Game Controller").GetComponent<MainGameController>();
    }

    // Update is called once per frame
    void Update()
    {

        //Check if all players are done with a level
        bool all_players_are_done = true;
        foreach (DictionaryEntry entity_entry in MAIN_GAME_CONTROLLER.players_hash)
        {
            GameObject player_object = (GameObject)(entity_entry.Value);
            if (player_object.GetComponent<PlayerScript>().play_state != PlayerScript.PLAY_STATE.in_the_hole)
            {
                all_players_are_done = false;
                continue;
            }
        }
        if (all_players_are_done)
            foreach (DictionaryEntry entity_entry in MAIN_GAME_CONTROLLER.players_hash)
            {
                GameObject player_object = (GameObject)(entity_entry.Value);
                current_level++;
                player_object.GetComponent<PlayerScript>().next_level();
            }
    }

    public Transform get_start_transform_for_next_level()
    {
        Transform parent_transform = LEVELS[current_level % LEVELS.Length].transform;

        for (int i = 0; i < parent_transform.childCount; i++)
        {
            if (parent_transform.GetChild(i).gameObject.tag == "Start")
                return parent_transform.GetChild(i);
        }
        return null;
    }
}
