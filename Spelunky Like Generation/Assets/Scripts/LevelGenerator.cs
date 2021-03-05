using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Transform[] startingPositions;
    public GameObject[] startingRooms;
    public GameObject[] rooms;

    public float moveAmount;
    private float currentTimeBetweenRooms;
    public float timeBetweenSpawns = 0.25f;

    private int direction;
    private int roomIndex;
    private int lastRoomIndex;
    private bool firstTime = true;
    [SerializeField]
    private bool skipRoom = false;

    // Start is called before the first frame update
    void Start()
    {
        int randomStartingPosition = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randomStartingPosition].position;

        int randomStartingRoom = Random.Range(0, startingRooms.Length);
        Instantiate(startingRooms[randomStartingRoom], transform.position, Quaternion.identity);
        roomIndex = randomStartingRoom;

        direction = Random.Range(1, 6);
    }

    private void Move()
    {
        if (direction == 1 || direction == 2)
        {
            // Move Right
            Vector2 newPosition = new Vector2(transform.position.x + moveAmount, transform.position.y);
            transform.position = newPosition;
        } else if (direction == 3 || direction == 4)
        {
            // Move Left
            Vector2 newPosition = new Vector2(transform.position.x - moveAmount, transform.position.y);
            transform.position = newPosition;
        } else if (direction == 5)
        {
            // Move Down
            if (firstTime)
            {
                if (rooms[lastRoomIndex].CompareTag("BottomExit") || startingRooms[lastRoomIndex].CompareTag("BottomExit"))
                {
                    Vector2 newPosition = new Vector2(transform.position.x, transform.position.y - moveAmount);
                    transform.position = newPosition;
                }
                else
                {
                    int otherChoice = Random.Range(1, 3);
                    if (otherChoice == 1)
                    {
                        // Move Right
                        Vector2 newPosition = new Vector2(transform.position.x + moveAmount, transform.position.y);
                        transform.position = newPosition;
                    }
                    else
                    {
                        // Move Left
                        Vector2 newPosition = new Vector2(transform.position.x - moveAmount, transform.position.y);
                        transform.position = newPosition;
                    }
                }
            } else
            {
                checkForDownPlacement();
            }
        }

        Collider2D collider = gameObject.GetComponent<Collider2D>();
        Collider2D collidedCollider = Physics2D.OverlapCircle(collider.transform.position, 3);

        if (collidedCollider != null && collidedCollider.gameObject.GetComponent<ExtraTags>().tags.Contains("Room"))
        {
            skipRoom = true;
        }

        lastRoomIndex = roomIndex;
        roomIndex = Random.Range(0, rooms.Length);

        firstTime = false;

        if (!skipRoom)
        {
            Instantiate(rooms[roomIndex], transform.position, Quaternion.identity);
        } else
        {
            skipRoom = false;
        }

        direction = Random.Range(1, 6);
    }

    private void checkForDownPlacement()
    {
        if (rooms[lastRoomIndex].CompareTag("BottomExit"))
        {
            Vector2 newPosition = new Vector2(transform.position.x, transform.position.y - moveAmount);
            transform.position = newPosition;
        }
        else
        {
            int otherChoice = Random.Range(1, 3);
            if (otherChoice == 1)
            {
                // Move Right
                Vector2 newPosition = new Vector2(transform.position.x + moveAmount, transform.position.y);
                transform.position = newPosition;
            }
            else
            {
                // Move Left
                Vector2 newPosition = new Vector2(transform.position.x - moveAmount, transform.position.y);
                transform.position = newPosition;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(currentTimeBetweenRooms <= 0)
        {
            Move();
            currentTimeBetweenRooms = timeBetweenSpawns;
        } else
        {
            currentTimeBetweenRooms -= Time.deltaTime;
        }
    }
}
