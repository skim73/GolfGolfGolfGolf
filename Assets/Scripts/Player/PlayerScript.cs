<<<<<<< HEAD:Assets/Scripts/PlayerScript.cs
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class PlayerScript : NetworkBehaviour
{
    public GameObject CAMERA_OBJ;
    public GameObject ROTATOR;
    public GameObject CLUB_MODEL;
    public GameObject GOLF_CLUB;
    public GameObject BALL;
    public GameObject POINTER;
    public GameObject START;
    public GameObject GOAL;
    public String PLAYER_NAME;
    public Color BALL_COLOR;
    public float TIME_TILL_DEATH = 2.5f;
    public float THRUST_MULTIPLIER = 3.0f;
    public float ROTATE_STRENGTH = 10.0f;
    public float MAX_STRENGTH = 40;
    public float STRENGTH_THRESHOLD = 1;
    public float POINTER_LENGTH = 2.5e-03f;
    public float HIT_TIMER = .05f;
    public float BALL_STOPPING_SPEED = .5f;
    public PLAY_STATE play_state = PLAY_STATE.waiting_for_player;

    public PowerUp power_up;

    private float strength = 0.0f;
    private float left_mouse_x = 0.0f;
    private float left_mouse_y = 0.0f;
    private float right_mouse_x = 0.0f;
    private float right_mouse_y = 0.0f;
    private float death_zone_timer = 0.0f;
    private Vector3 last_ball_pos;
    public Vector3 last_valid_position;
    private float timer = 1.0f;
    private float last_strength = 0.0f;
    private float last_camera_angle_x = 0.0f;
    private float last_camera_angle_y = 0.0f;
    public bool in_death_zone = false;
    private bool left_mouse_clicked = false;
    private bool right_mouse_clicked = false;
    private bool left_arrow_clicked = false;
    private bool right_arrow_clicked = false;
    private bool w_clicked = false;
    private bool a_clicked = false;
    private bool s_clicked = false;
    private bool d_clicked = false;
    private bool moving_club = false;
    private bool add_impulse = false;
    private float rest_timer = 0f;
    private Rigidbody ball_rb;
    private float actual_time_swinging = 0f;

    // Sabin Kim: (using_fireproof : bool) : true if player currently using fireproof powerup
    private bool using_fireproof = false;

    public enum PLAY_STATE
    {
        waiting_for_player,
        hitting_ball,
        ball_rolling,
        in_the_hole
    }

    // Start is called before the first frame update
    public override void OnStartAuthority()
    {
        START = GameObject.Find("Start");
        this.transform.position = START.transform.position + new Vector3(0, .05f, 0);
        this.transform.rotation = START.transform.rotation;
        BALL.transform.position = START.transform.position + new Vector3(0, .05f, 0);
        ball_rb = BALL.GetComponent<Rigidbody>();
        ball_rb.maxAngularVelocity = 100000;
        BALL.GetComponent<MeshRenderer>().material.color = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );
        POINTER.transform.localScale = new Vector3(5 * POINTER_LENGTH, 1, POINTER_LENGTH);
        POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.1f, 0);
        _next_turn();
    }

    // Update is called once per frame
    void Update()
    {
        if (!base.hasAuthority)
        {
            return;
        }
        _handle_right_click();
        _handle_ws();
        _handle_ad();
        switch (play_state)
        {
            case PLAY_STATE.waiting_for_player:
                if (in_death_zone)
                {
                    _resetOnDeath();
                    break;
                }
                if (using_fireproof && !(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                ball_rb.velocity = Vector3.zero;
                _handle_left_click();
                _handle_arrow_keys();
                break;
            case PLAY_STATE.hitting_ball:
                if (timer > 0)
                {
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                    last_ball_pos = BALL.transform.position;
                    add_impulse = true;
                    play_state = PLAY_STATE.ball_rolling;
                    moving_club = true;
                }
                break;
            case PLAY_STATE.ball_rolling:
                if (in_death_zone && death_zone_timer <= 0)
                {
                    _resetOnDeath();
                    break;
                }
                if (in_death_zone)
                    death_zone_timer -= Time.deltaTime;
                Vector3 diff_ball_pos = BALL.transform.position - last_ball_pos;
                Vector3 cam_pos = CAMERA_OBJ.transform.position;
                _handle_space_bar();
                CAMERA_OBJ.transform.position = cam_pos + diff_ball_pos;
                if (timer > -1 * HIT_TIMER)
                {
                    actual_time_swinging += Time.deltaTime;
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else if (timer > -4 * HIT_TIMER)
                {
                    timer = timer - Time.deltaTime;
                }
                else if (moving_club)
                {
                    moving_club = false;
                    GOLF_CLUB.transform.Rotate(0, last_strength * actual_time_swinging / HIT_TIMER, 0);
                    Cmd_disable_golf_club();
                }
                else if (rest_timer > 2.0f)
                {
                    actual_time_swinging = 0f;
                    ball_rb.velocity = Vector3.zero;
                    _next_turn();
                }
                else if (ball_rb.velocity.magnitude < BALL_STOPPING_SPEED && ball_rb.velocity.y < 0.1)
                {
                    rest_timer = rest_timer + Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                }
                last_ball_pos = BALL.transform.position;
                break;
            case PLAY_STATE.in_the_hole:
                if (!(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (add_impulse == true)
        {
            float thrust = last_strength * THRUST_MULTIPLIER;
            ball_rb.AddForce(ROTATOR.transform.right * thrust);
            add_impulse = false;
        }
    }

    private void _next_turn()
    {
        Cmd_enable_golf_club();
        last_strength = 0;
        ROTATOR.transform.position = BALL.transform.position;
        last_valid_position = BALL.transform.position;
        play_state = PLAY_STATE.waiting_for_player;
    }

    private void _handle_arrow_keys()
    {
        if (left_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                left_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.up * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        else if (right_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                right_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.down * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        if (!right_arrow_clicked && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            left_arrow_clicked = true;
        }

        if (!left_arrow_clicked && Input.GetKeyDown(KeyCode.RightArrow))
        {
            right_arrow_clicked = true;
        }
    }

    private void _handle_left_click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            left_mouse_clicked = true;
            left_mouse_x = Input.mousePosition.x;
            left_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0) && left_mouse_clicked)
        {
            if (last_strength > STRENGTH_THRESHOLD)
            {
                play_state = PLAY_STATE.hitting_ball;
                timer = HIT_TIMER;
            }
            left_mouse_clicked = false;
            left_mouse_x = 0;
            left_mouse_y = 0;
        }
        else if (left_mouse_clicked)
        {
            float strength = Math.Min(Math.Abs(Input.mousePosition.y - left_mouse_y) / 15, MAX_STRENGTH);
            POINTER.transform.localScale = new Vector3(strength * POINTER_LENGTH, 1, POINTER_LENGTH);
            POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.1f, 0);
            GOLF_CLUB.transform.Rotate(0, strength - last_strength, 0);

            last_strength = strength;
        }
    }

    [Command]
    void Cmd_disable_golf_club()
    {
        Rpc_disable_golf_club();
    }

    [ClientRpc]
    void Rpc_disable_golf_club()
    {
        ROTATOR.SetActive(false);
    }

    [Command]
    void Cmd_enable_golf_club()
    {
        Rpc_enable_golf_club();
    }

    [ClientRpc]
    void Rpc_enable_golf_club()
    {
        ROTATOR.SetActive(true);
    }

    private void _handle_right_click()
    {
        if (Input.GetMouseButtonDown(1))
        {
            right_mouse_clicked = true;
            right_mouse_x = Input.mousePosition.x;
            right_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(1) && right_mouse_clicked)
        {
            right_mouse_clicked = false;
            right_mouse_x = 0;
            right_mouse_y = 0;
            last_camera_angle_x = 0f;
            last_camera_angle_y = 0f;
        }
        else if (right_mouse_clicked)
        {
            float curr_x_angle = (Input.mousePosition.x - right_mouse_x) / 10;
            float curr_y_angle = (Input.mousePosition.y - right_mouse_y) / -10;
            CAMERA_OBJ.transform.Rotate(curr_y_angle - last_camera_angle_y, curr_x_angle - last_camera_angle_x, 0, Space.World);

            last_camera_angle_x = curr_x_angle;
            last_camera_angle_y = curr_y_angle;
        }
    }

    private void _handle_ws()
    {
        if (w_clicked)
        {
            if (Input.GetKeyUp(KeyCode.W))
            {
                w_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.forward * Time.deltaTime, Space.World);
            }
        }
        else if (s_clicked)
        {
            if (Input.GetKeyUp(KeyCode.S))
            {
                s_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2f * Vector3.forward * Time.deltaTime, Space.World);
            }
        }
        if (!s_clicked && Input.GetKeyDown(KeyCode.W))
        {
            w_clicked = true;
        }

        if (!w_clicked && Input.GetKeyDown(KeyCode.S))
        {
            s_clicked = true;
        }
    }

    private void _handle_ad()
    {
        if (a_clicked)
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                a_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2 * Vector3.right * Time.deltaTime, Space.World);
            }
        }
        else if (d_clicked)
        {
            if (Input.GetKeyUp(KeyCode.D))
            {
                d_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.right * Time.deltaTime, Space.World);
            }
        }
        if (!d_clicked && Input.GetKeyDown(KeyCode.A))
        {
            a_clicked = true;
        }

        if (!a_clicked && Input.GetKeyDown(KeyCode.D))
        {
            d_clicked = true;
        }
    }

    public void reachedGoal()
    {
        play_state = PLAY_STATE.in_the_hole;
    }

    public void _handle_space_bar()
    {
        if (this.power_up != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Using Powerup: " + power_up.name);
                this.power_up.onUse(BALL);
                if (this.power_up is FireProofPowerUp)
                {
                    using_fireproof = true;
                }
                this.power_up = null;

                GameObject powerUpCanvas = GameObject.FindGameObjectWithTag("PowerUpCanvas");
                powerUpCanvas.GetComponentInChildren<Text>().text = "Power Up: \nNULL";
                Debug.Log(powerUpCanvas.GetComponentInChildren<Text>().text);
            }
        }
    }

    public void pickedUpPowerUp(PowerUp power_up)
    {
        this.power_up = power_up;
    }
    private void _resetOnDeath()
    {
        Debug.Log("reseting from death");
        ball_rb.velocity = Vector3.zero;
        CAMERA_OBJ.transform.position = CAMERA_OBJ.transform.position + last_valid_position - BALL.transform.position;
        BALL.transform.position = last_valid_position;
        _next_turn();
        in_death_zone = false;
        _next_turn();
    }
    public void enterDeathZone()
    {
        in_death_zone = true;
        death_zone_timer = TIME_TILL_DEATH;
    }
    public void exitDeathZone()
    {
        in_death_zone = false;
    }

    // Sabin Kim: getter for bool using_fireproof
    public bool isUsingFireProof()
    {
        return using_fireproof;
    }
}
=======
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;
using Random = UnityEngine.Random;

public class PlayerScript : NetworkBehaviour
{
    public GameObject CAMERA_OBJ;
    public GameObject ROTATOR;
    public GameObject CLUB_MODEL;
    public GameObject GOLF_CLUB;
    public GameObject BALL;
    public GameObject POINTER;
    public GameWorldController WORLD_MANAGER;
    public String PLAYER_NAME = "Host";
    public Color BALL_COLOR;
    public float TIME_TILL_DEATH = 2.5f;
    public float THRUST_MULTIPLIER = 3.0f;
    public float ROTATE_STRENGTH = 10.0f;
    public float MAX_STRENGTH = 40;
    public float STRENGTH_THRESHOLD = 1;
    public float POINTER_LENGTH = 2.5e-03f;
    public float HIT_TIMER = .05f;
    public float BALL_STOPPING_SPEED = .5f;
    public PLAY_STATE play_state = PLAY_STATE.waiting_for_player;
    public PowerUp power_up;
    public int score = -1;

    private float strength = 0.0f;
    private float left_mouse_x = 0.0f;
    private float left_mouse_y = 0.0f;
    private float right_mouse_x = 0.0f;
    private float right_mouse_y = 0.0f;
    private float death_zone_timer = 0.0f;
    private Vector3 last_ball_pos;
    public Vector3 last_valid_position;
    private float timer = 1.0f;
    private float last_strength = 0.0f;
    private float last_camera_angle_x = 0.0f;
    private float last_camera_angle_y = 0.0f;
    public bool in_death_zone = false;
    private bool left_mouse_clicked = false;
    private bool right_mouse_clicked = false;
    private bool left_arrow_clicked = false;
    private bool right_arrow_clicked = false;
    private bool w_clicked = false;
    private bool a_clicked = false;
    private bool s_clicked = false;
    private bool d_clicked = false;
    private bool moving_club = false;
    private bool add_impulse = false;
    private float rest_timer = 0f;
    private Rigidbody ball_rb;
    private float actual_time_swinging = 0f;

    // Sabin Kim: (using_fireproof : bool) : true if player currently using fireproof powerup
    private bool using_fireproof = false;

    public enum PLAY_STATE
    {
        waiting_for_player,
        hitting_ball,
        ball_rolling,
        in_the_hole
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("Game World Controller"))
            WORLD_MANAGER = GameObject.Find("Game World Controller").GetComponent<GameWorldController>();
        POINTER.transform.localScale = new Vector3(5 * POINTER_LENGTH, 1, POINTER_LENGTH);
        POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.15f, 0);
        if (WORLD_MANAGER)
            next_level();
    }

    // Update is called once per frame
    void Update()
    {
        if (WORLD_MANAGER == false)
            return;
        if (ball_rb == false)
        {
            ball_rb = BALL.GetComponent<Rigidbody>();
            ball_rb.maxAngularVelocity = 100000;
        }
        BALL.GetComponent<MeshRenderer>().material.color = BALL_COLOR;
        if (!base.hasAuthority)
            return;
        //Tell all other clients your game state
        Cmd_publish_game_state(this.play_state);
        _handle_right_click();
        _handle_ws();
        _handle_ad();
        switch (play_state)
        {
            case PLAY_STATE.waiting_for_player:
                if (in_death_zone)
                {
                    _resetOnDeath();
                    break;
                }
                if (using_fireproof && !(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                ball_rb.velocity = Vector3.zero;
                _handle_left_click();
                _handle_arrow_keys();
                break;
            case PLAY_STATE.hitting_ball:
                Debug.Log("Hitting");
                if (timer > 0)
                {
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                    last_ball_pos = BALL.transform.position;
                    add_impulse = true;
                    play_state = PLAY_STATE.ball_rolling;
                    moving_club = true;
                }
                break;
            case PLAY_STATE.ball_rolling:
                if (in_death_zone && death_zone_timer <= 0)
                {
                    _resetOnDeath();
                    break;
                }
                if (in_death_zone)
                    death_zone_timer -= Time.deltaTime;
                Vector3 diff_ball_pos = BALL.transform.position - last_ball_pos;
                Vector3 cam_pos = CAMERA_OBJ.transform.position;
                _handle_space_bar();
                CAMERA_OBJ.transform.position = cam_pos + diff_ball_pos;
                if (timer > -1 * HIT_TIMER)
                {
                    actual_time_swinging += Time.deltaTime;
                    GOLF_CLUB.transform.Rotate(0, -1 * last_strength * Time.deltaTime / HIT_TIMER, 0);
                    timer = timer - Time.deltaTime;
                }
                else if (timer > -4 * HIT_TIMER)
                {
                    timer = timer - Time.deltaTime;
                }
                else if (moving_club)
                {
                    moving_club = false;
                    GOLF_CLUB.transform.Rotate(0, last_strength * actual_time_swinging / HIT_TIMER, 0);
                    Cmd_disable_golf_club();
                }
                else if (rest_timer > 2.0f)
                {
                    actual_time_swinging = 0f;
                    ball_rb.velocity = Vector3.zero;
                    _next_turn();
                }
                else if (ball_rb.velocity.magnitude < BALL_STOPPING_SPEED && ball_rb.velocity.y < 0.1)
                {
                    rest_timer = rest_timer + Time.deltaTime;
                }
                else
                {
                    rest_timer = 0f;
                }
                last_ball_pos = BALL.transform.position;
                break;
            case PLAY_STATE.in_the_hole:
                if (!(power_up is FireProofPowerUp))
                {
                    using_fireproof = false;
                }
                break;
        }
    }

    private void FixedUpdate()
    {
        if (add_impulse == true)
        {
            float thrust = last_strength * THRUST_MULTIPLIER;
            ball_rb.AddForce(ROTATOR.transform.right * thrust);
            add_impulse = false;
        }
    }

    private void _next_turn()
    {
        Cmd_enable_golf_club();
        Cmd_increase_score();
        last_strength = 0;
        ROTATOR.transform.position = BALL.transform.position;
        last_valid_position = BALL.transform.position;
        play_state = PLAY_STATE.waiting_for_player;
    }

    public void next_level()
    {
        Transform start = WORLD_MANAGER.get_start_transform_for_next_level();
        this.transform.position = start.position + new Vector3(0, .05f, 0);
        this.transform.localRotation = start.rotation;
        ROTATOR.transform.localRotation = Quaternion.identity;
        BALL.transform.position = start.position + new Vector3(0, .05f, 0);
        _next_turn();
    }

    private void _handle_arrow_keys()
    {
        if (left_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                left_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.up * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        else if (right_arrow_clicked)
        {
            if (Input.GetKeyUp(KeyCode.RightArrow))
            {
                right_arrow_clicked = false;
            }
            else
            {
                ROTATOR.transform.Rotate(Vector3.down * ROTATE_STRENGTH * Time.deltaTime);
            }
        }
        if (!right_arrow_clicked && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            left_arrow_clicked = true;
        }

        if (!left_arrow_clicked && Input.GetKeyDown(KeyCode.RightArrow))
        {
            right_arrow_clicked = true;
        }
    }

    private void _handle_left_click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            left_mouse_clicked = true;
            left_mouse_x = Input.mousePosition.x;
            left_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(0) && left_mouse_clicked)
        {
            if (last_strength > STRENGTH_THRESHOLD)
            {
                this.play_state = PLAY_STATE.hitting_ball;
                timer = HIT_TIMER;
            }
            left_mouse_clicked = false;
            left_mouse_x = 0;
            left_mouse_y = 0;
        }
        else if (left_mouse_clicked)
        {
            float strength = Math.Min(Math.Abs(Input.mousePosition.y - left_mouse_y) / 15, MAX_STRENGTH);
            POINTER.transform.localScale = new Vector3(strength * POINTER_LENGTH, 1, POINTER_LENGTH);
            POINTER.transform.localPosition = new Vector3(POINTER.transform.localScale.x * 5, BALL.transform.localScale.y / -2.1f, 0);
            GOLF_CLUB.transform.Rotate(0, strength - last_strength, 0);

            last_strength = strength;
        }
    }

    private void _handle_right_click()
    {
        if (Input.GetMouseButtonDown(1))
        {
            right_mouse_clicked = true;
            right_mouse_x = Input.mousePosition.x;
            right_mouse_y = Input.mousePosition.y;
        }
        else if (Input.GetMouseButtonUp(1) && right_mouse_clicked)
        {
            right_mouse_clicked = false;
            right_mouse_x = 0;
            right_mouse_y = 0;
            last_camera_angle_x = 0f;
            last_camera_angle_y = 0f;
        }
        else if (right_mouse_clicked)
        {
            float curr_x_angle = (Input.mousePosition.x - right_mouse_x) / 10;
            float curr_y_angle = (Input.mousePosition.y - right_mouse_y) / -10;
            CAMERA_OBJ.transform.Rotate(curr_y_angle - last_camera_angle_y, curr_x_angle - last_camera_angle_x, 0, Space.World);

            last_camera_angle_x = curr_x_angle;
            last_camera_angle_y = curr_y_angle;
        }
    }

    private void _handle_ws()
    {
        if (w_clicked)
        {
            if (Input.GetKeyUp(KeyCode.W))
            {
                w_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.forward * Time.deltaTime, Space.World);
            }
        }
        else if (s_clicked)
        {
            if (Input.GetKeyUp(KeyCode.S))
            {
                s_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2f * Vector3.forward * Time.deltaTime, Space.World);
            }
        }
        if (!s_clicked && Input.GetKeyDown(KeyCode.W))
        {
            w_clicked = true;
        }

        if (!w_clicked && Input.GetKeyDown(KeyCode.S))
        {
            s_clicked = true;
        }
    }

    private void _handle_ad()
    {
        if (a_clicked)
        {
            if (Input.GetKeyUp(KeyCode.A))
            {
                a_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(-2 * Vector3.right * Time.deltaTime, Space.World);
            }
        }
        else if (d_clicked)
        {
            if (Input.GetKeyUp(KeyCode.D))
            {
                d_clicked = false;
            }
            else
            {
                CAMERA_OBJ.transform.Translate(2 * Vector3.right * Time.deltaTime, Space.World);
            }
        }
        if (!d_clicked && Input.GetKeyDown(KeyCode.A))
        {
            a_clicked = true;
        }

        if (!a_clicked && Input.GetKeyDown(KeyCode.D))
        {
            d_clicked = true;
        }
    }

    public void reachedGoal()
    {
        play_state = PLAY_STATE.in_the_hole;
    }

    public void _handle_space_bar()
    {
        if (this.power_up != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Using Powerup: " + power_up.name);
                this.power_up.onUse(BALL);
                if (this.power_up is FireProofPowerUp)
                {
                    using_fireproof = true;
                }
                this.power_up = null;
            }
        }
    }

    public void pickedUpPowerUp(PowerUp power_up) { this.power_up = power_up; }

    private void _resetOnDeath()
    {
        Debug.Log("reseting from death");
        ball_rb.velocity = Vector3.zero;
        CAMERA_OBJ.transform.position = CAMERA_OBJ.transform.position + last_valid_position - BALL.transform.position;
        BALL.transform.position = last_valid_position;
        _next_turn();
        in_death_zone = false;
        _next_turn();
    }
    public void enterDeathZone()
    {
        in_death_zone = true;
        death_zone_timer = TIME_TILL_DEATH;
    }
    public void exitDeathZone() { in_death_zone = false; }

    // Sabin Kim: getter for bool using_fireproof
    public bool isUsingFireProof() { return using_fireproof; }


    /*************************************************
     * Command functions are called by the player with authority
     * They call the ClientRpc method of all other clients
     * This is how all information has to be passed from 1 client to another
     *************************************************/
    [Command]
    public void Cmd_set_name_and_color(string name, Color color) { Rpc_set_name_and_color(name, color); }

    [ClientRpc]
    void Rpc_set_name_and_color(string name, Color color) { PLAYER_NAME = name; BALL_COLOR = color; BALL.GetComponent<MeshRenderer>().material.color = BALL_COLOR; }

    [Command]
    void Cmd_disable_golf_club() { Rpc_disable_golf_club(); }

    [ClientRpc]
    void Rpc_disable_golf_club() { ROTATOR.SetActive(false); }

    [Command]
    void Cmd_enable_golf_club() { Rpc_enable_golf_club(); }

    [ClientRpc]
    void Rpc_enable_golf_club() { ROTATOR.SetActive(true); }

    [Command]
    void Cmd_increase_score() { Rpc_increase_score(); }

    [ClientRpc]
    void Rpc_increase_score() { this.score++; }

    [Command]
    public void Cmd_publish_game_state( PLAY_STATE play_state ) { Rpc_publish_game_state(play_state); }

    [ClientRpc]
    public void Rpc_publish_game_state(PLAY_STATE play_state) { if (!base.hasAuthority ) this.play_state = play_state; }

}
>>>>>>> upstream/master:Assets/Scripts/Player/PlayerScript.cs
