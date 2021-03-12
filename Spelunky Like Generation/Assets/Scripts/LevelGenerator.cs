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

    public static int arrayXSize = 16;
    public static int arrayYSize = 16;
    public int[,] roomIndexList = new int[arrayXSize, arrayYSize];

    private int direction;
    [SerializeField]
    private int roomIndex;
    private int lastRoomIndex;
    private bool firstTime = true;
    [SerializeField]
    private bool skipRoom = false;
    private bool generationDone = false;

    [SerializeField]
    private List<int> bottomRoomIndices = new List<int>();
    private List<int> topRoomIndices = new List<int>();
    private List<int> topBottomRoomIndices = new List<int>();

    private bool topRoomNeeded = false;

    // Start is called before the first frame update
    void Start()
    {
        int randomStartingPosition = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randomStartingPosition].position;

        int randomStartingRoom = Random.Range(0, startingRooms.Length);
        Instantiate(startingRooms[randomStartingRoom], transform.position, Quaternion.identity);
        roomIndex = randomStartingRoom;

        for(int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].name == startingRooms[randomStartingRoom].name)
            {
                addToGrid(i);
            }
        }

        direction = Random.Range(1, 6);

        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i].CompareTag("BottomExit"))
            {
                bottomRoomIndices.Add(i);
            } else if (rooms[i].GetComponent<ExtraTags>().tags.Contains("TopExit"))
            {
                topRoomIndices.Add(i);
            } else if (rooms[i].GetComponent<ExtraTags>().tags.Contains("TopBottomExit"))
            {
                topBottomRoomIndices.Add(i);
            }
        }
    }

    private void Move()
    {
        Vector2 lastPosition = transform.position;

        Debug.LogWarning("New Room: " + direction);

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

        if (transform.position.x == maxX || transform.position.x == minX)
        {
            Debug.Log("2: " + rooms[roomIndex].name + roomIndex + " | " + direction);
            roomIndex = bottomRoomIndices[Random.Range(0, bottomRoomIndices.Count)];
            direction = 6;
            Debug.Log("3: " + roomIndex + " | " + direction);
        } else if(transform.position.x > maxX || transform.position.x < minX)
        {
            Debug.Log("4: " + roomIndex + " | " + direction);
            transform.position = lastPosition;
            Collider2D[] lastRooms = Physics2D.OverlapCircleAll(transform.position, 3);
            Destroy(lastRooms[1].gameObject);
            
            // This checks if the room above has a bottom exit. If it does, it means we need a top and bottom exit for this one.
            (int gridPosX, int gridPosY) = getCurrentGridPos();
            if (rooms[roomIndexList[gridPosX, gridPosY - 1]].CompareTag("BottomExit"))
            {
                roomIndex = topBottomRoomIndices[Random.Range(0, topBottomRoomIndices.Count)];
            }
            else
            {
                roomIndex = bottomRoomIndices[Random.Range(0, bottomRoomIndices.Count)];
            }

            direction = 6;
            Debug.Log("5: " + roomIndex + " | " + direction);
        }

        if (topRoomNeeded)
        {
            Debug.Log("6: " + roomIndex + " | " + direction);
            Debug.Log("top room was needed");
            topRoomNeeded = false;

            if (!rooms[roomIndex].GetComponent<ExtraTags>().tags.Contains("TopExit"))
            {
                roomIndex = topRoomIndices[Random.Range(0, topRoomIndices.Count)];
            }
            
            if(transform.position.x >= maxX)
            {
                direction = 3;
            } else
            {
                direction = 1;
            }
            Debug.Log("7: " + roomIndex + " | " + direction);
        }

        if (topRoomNeeded && direction == 6)
        {
            Debug.Log("8: " + roomIndex + " | " + direction);
            topRoomNeeded = false;
            roomIndex = topBottomRoomIndices[Random.Range(0, topBottomRoomIndices.Count)];

            if (transform.position.x >= maxX)
            {
                direction = 3;
            }
            else
            {
                direction = 1;
            }
            Debug.Log("9: " + roomIndex + " | " + direction);
        }

        if (bottomRoomIndices.Contains(roomIndex) && transform.position.y == -15 && !skipRoom)
        {
            // The generation is not actually done as we need to fill in the rest of the grid but for now this stops it going out
            generationDone = true;
        } else if(transform.position.y < -15)
        {
            Debug.Log("10: " + roomIndex + " | " + direction);
            generationDone = true;
            transform.position = lastPosition;
            Collider2D[] lastRooms = Physics2D.OverlapCircleAll(transform.position, 3);
            Destroy(lastRooms[1].gameObject);
            roomIndex = bottomRoomIndices[Random.Range(0, bottomRoomIndices.Count)];
            Debug.Log("11: " + roomIndex + " | " + direction);
        }

        if (!skipRoom)
        {
            //GameObject room = Instantiate(rooms[roomIndex], transform.position, Quaternion.identity);

            Debug.Log("Final: " + rooms[roomIndex].name + roomIndex + " | " + direction);
            Instantiate(rooms[roomIndex], transform.position, Quaternion.identity);

            addToGrid(roomIndex);
        }
        else
        {
            direction = Random.Range(1, 5);
            skipRoom = false;
        }
    }

    private void checkForDownPlacement()
    {
        if (rooms[lastRoomIndex].CompareTag("BottomExit"))
        {
            Debug.Log("Moving Down");
            Vector2 newPosition = new Vector2(transform.position.x, transform.position.y - moveAmount);
            transform.position = newPosition;
            topRoomNeeded = true;
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

    private void addToGrid(int specificRoomIndex)
    {
        /*X:  -25 -15 -5 5
            *Y:  # 3 2 1 0
            *15  0 
            *5   1
            *-5  2
            *-15 3
            */

        float gridPosX = transform.position.x / 10;

        if (gridPosX == 0.5)
        {
            gridPosX = 0;
        }
        else
        {
            gridPosX = Mathf.Ceil(Mathf.Abs(gridPosX));
        }

        float gridPosY = transform.position.y / 10;

        if (gridPosY == 1.5)
        {
            gridPosY = 0;
        }
        else if (gridPosY == 0.5)
        {
            gridPosY = 1;
        }
        else
        {
            // This rounds the number which is either -0.5 or -1.5 minus 1 so it would equal 2 or 3 as it would round -1.5 to 2 and then -2.5 to 3
            gridPosY = Mathf.Abs(Mathf.Ceil(gridPosY - 1));
        }

         //Debug.Log(rooms[specificRoomIndex].name + ": " + gridPosX + ", " + gridPosY);

        roomIndexList[(int) gridPosX, (int) gridPosY] = specificRoomIndex;
    }

    private (int gridPosX, int gridPosY) getCurrentGridPos()
    {
        float gridPosX = transform.position.x / 10;

        if (gridPosX == 0.5)
        {
            gridPosX = 0;
        }
        else
        {
            gridPosX = Mathf.Ceil(Mathf.Abs(gridPosX));
        }

        float gridPosY = transform.position.y / 10;

        if (gridPosY == 1.5)
        {
            gridPosY = 0;
        }
        else if (gridPosY == 0.5)
        {
            gridPosY = 1;
        }
        else
        {
            // This rounds the number which is either -0.5 or -1.5 minus 1 so it would equal 2 or 3 as it would round -1.5 to 2 and then -2.5 to 3
            gridPosY = Mathf.Abs(Mathf.Ceil(gridPosY - 1));
        }

        return ((int) gridPosX, (int) gridPosY);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimeBetweenRooms <= 0 && !generationDone)
        {
            Move();
            currentTimeBetweenRooms = timeBetweenSpawns;
        } else if (generationDone)
        {
            //Destroy(gameObject);
        }
        else
        {
            currentTimeBetweenRooms -= Time.deltaTime;
        }
    }
}
