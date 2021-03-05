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
    public float maxX = 5f;
    public float minX = -25f;
    // Layers go between 1 and 4
    // 1 is the top while 4 is the bottom
    public int layer = 1;

    private int direction;
    private int roomIndex;
    private int lastRoomIndex;
    private bool firstTime = true;
    [SerializeField]
    private bool skipRoom = false;

    private List<int> bottomRoomIndices = new List<int>();
    private List<int> topRoomIndices = new List<int>();

    private bool topRoomNeeded = false;

    // Start is called before the first frame update
    void Start()
    {
        int randomStartingPosition = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randomStartingPosition].position;

        int randomStartingRoom = Random.Range(0, startingRooms.Length);
        Instantiate(startingRooms[randomStartingRoom], transform.position, Quaternion.identity);
        roomIndex = randomStartingRoom;

        direction = Random.Range(1, 6);

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].CompareTag("BottomExit"))
            {
                bottomRoomIndices.Add(i);
            } else if (rooms[i].GetComponent<ExtraTags>().tags.Contains("TopExit"))
            {
                topRoomIndices.Add(i);
            }
        }
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
        } else if (direction == 6)
        {
            // Force Move Down
            // For the direction to be this, the above room must be a down room so this must be an up room of some kind
            Vector2 newPosition = new Vector2(transform.position.x, transform.position.y - moveAmount);
            transform.position = newPosition;
            topRoomNeeded = true;
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

        direction = Random.Range(1, 6);

        if (transform.position.x >= maxX || transform.position.x <= minX)
        {
            roomIndex = bottomRoomIndices[Random.Range(0, bottomRoomIndices.Count)];
            direction = 6;
        }

        if (topRoomNeeded)
        {
            topRoomNeeded = false;
            roomIndex = topRoomIndices[Random.Range(0, topRoomIndices.Count)];
            
            if(transform.position.x >= maxX)
            {
                direction = 3;
            } else
            {
                direction = 1;
            }
        }

        if (!skipRoom)
        {
            Instantiate(rooms[roomIndex], transform.position, Quaternion.identity);
        } else
        {
            skipRoom = false;
        }
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
