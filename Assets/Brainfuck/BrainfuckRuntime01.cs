using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class BrainfuckRuntime01 : MonoBehaviour
{
    // Runtime
    const int MEMSIZE = 30000;
    const int INSBUFFERSIZE = 10000;
    byte[] memory;
    byte[] insBuffer;
    int insPointer = 0;
    int dataPointer = 0;
    bool isExecuting = false;
    bool isAcceptingInput = false;

    // UI
    [SerializeField] InputField insInput;
    [SerializeField] InputField userInput;
    [SerializeField] Text userOutput;
    [SerializeField] Image isExecutingImg;
    [SerializeField] Image isAcceptingInputImg;
    [SerializeField] Color TrueColor;
    [SerializeField] Color FalseColor;


    // Start is called before the first frame update
    void Start()
    {
        UpdateImgUI();
        // DebugStuff();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartRuntime();
        }

        if(!isExecuting) return;

        // if(!isAcceptingInput)
        // {
        //     // Iterate over instruction buffer until 0 value is reached.
        //     for(int i = 0; i < INSBUFFERSIZE; i++)
        //     {
        //         if(insBuffer[i] == 0) { isExecuting = false; break; }
        //         ExecuteInstruction(i);

        //         // If interating over last instruction, stop execution.
        //         if(i == INSBUFFERSIZE-1) isExecuting = false;
        //     }
        // }

        if(!isAcceptingInput)
        {
            // Iterate over instruction buffer.
            ExecuteInstruction();

            // If interating over last instruction, stop execution.
            if(insPointer == INSBUFFERSIZE-1)
            {
                isExecuting = false;
                UpdateImgUI();
            } 
        }
    }

    void ExecuteInstruction()
    {
        if(!isExecuting) return;

        switch(insBuffer[insPointer])
        {
            case 0x3E: // '>' Increment Data pointer
                dataPointer = dataPointer < 255 ? dataPointer+1 : 255;
                break;
            case 0x3C: // '<' Decrement Data pointer
                dataPointer = dataPointer > 0 ? dataPointer-1 : 0;
                break;
            case 0x2B: // '+' Incrememnt memory[dataPointer]
                memory[dataPointer] = 
                    memory[dataPointer] < 255 ? (byte)(memory[dataPointer]+1) : (byte)255;
                // Debug.Log(memory[dataPointer]);
                break;
            case 0x2D: // '-' Decrememnt memory[dataPointer]
                memory[dataPointer] = 
                    memory[dataPointer] > 0 ? (byte)(memory[dataPointer]-1) : (byte)0;
                break;
            case 0x2E: // '.' Output
                userOutput.text += $"{Convert.ToString(memory[dataPointer], 16).PadLeft(2, '0')} ";
                break;
            case 0x2C: // ',' Input
                isAcceptingInput = true;
                userInput.interactable = true;
                UpdateImgUI();
                break;
            case 0x5B: // '[' - Jump forward to next ']' if memory[dataPointer] is 0
                // If dataPointer points to 0 value
                if(memory[dataPointer] == 0x0)
                {
                    // Start reading ins right after '['
                    for(int i = insPointer+1; i < INSBUFFERSIZE; i++)
                    {
                        // If ']' is found
                        if(insBuffer[i] == 0x5D)
                        { 
                            // Set insPointer to ']' location. After switch exits,
                            // insPointer gets incremented.
                            insPointer = i; break; 
                        }
                        // If ']' is not found, keep iterating to the end.
                    }
                }
                break;
            case 0x5D: // ']' - Jump backward to previous '[' if memory[dataPointer] is not 0
                // If dataPointer points to non-zero value
                if(memory[dataPointer] != 0)
                {
                    // Start reading backwards for a '[' instruction.
                    for(int i = insPointer-1; i > -1; i--)
                    {
                        // If '[' is found
                        if(insBuffer[i] == 0x5B)
                        {
                            insPointer = i; break;
                        }
                        // If '[' is not found, keep iterating to the start.
                    }
                }
                break;
            case 0x0: // Invalid instruction, halt execution.
                isExecuting = false;
                UpdateImgUI();
                break;
            default: break;
        }
        insPointer++;

    }

    void Init()
    {
        memory = new byte[MEMSIZE];
        insBuffer = new byte[INSBUFFERSIZE];
        insPointer = 0;
        dataPointer = 0;
        isExecuting = false;

        // char[] insCharArr = insInput.text.ToCharArray();
        char[] insCharArr = ValidateInsCharInput();
        byte[] tempByteArr = ASCIIEncoding.ASCII.GetBytes(insCharArr, 0, insCharArr.Length);
        Array.Copy(tempByteArr, insBuffer, tempByteArr.Length);
        // insBuffer = ASCIIEncoding.ASCII.GetBytes(insCharArr, 0, insCharArr.Length);

        userInput.text = "";
        userOutput.text = "";
        userInput.interactable = false;

        isExecuting = true;
        isAcceptingInput = false;
        UpdateImgUI();

    }

    char[] ValidateInsCharInput()
    {
        // Instructions
        char[] validInsChars = {'<','>','+','-','.',',','[',']'};
        
        // 0 = Newline, 1 = Comment
        char[] validOtherChars = {'\n','#'};

        string insInputText = insInput.text;
        
        // Strip comments
        for(int i = 0; i < insInputText.Length; i++)
        {
            // If # char is found
            if(insInputText[i] == validOtherChars[1])
            {
                // Make a new iteration and iterate till \n is found
                for(int j = i; j < insInputText.Length; j++)
                {
                    // If \n is found, this is a comment
                    if(insInputText[j] == validOtherChars[0])
                    {
                        // Remove chars from # to \n, basically removing the comment
                        insInputText = insInputText.Remove(i, (j-i)+1);
                        break;
                    }
                }
            }
        }

        // Strip invalid chars
        for(int i = 0; i < insInputText.Length; i++)
        {
            // If this flag never gets set to false, the char is invalid.
            bool invalidCharFound = true;

            for(int j = 0; j < validInsChars.Length; j++)
            {
                if(insInputText[i] == validInsChars[j]) invalidCharFound = false;
            }

            // If a char gets removed, the next char replaces the char at the current i 
            // index, so decrement i, so that the char doesn't get skipped over.
            if(invalidCharFound) { insInputText = insInputText.Remove(i,1); i--; }
        }

        // Strip whitespaces
        insInputText = insInputText.Replace(" ", "");
        
        Debug.Log(insInputText);

        return insInputText.ToCharArray();
    }

    public void StartRuntime()
    {
        Init();
    }

    void UpdateImgUI()
    {
        isExecutingImg.color = isExecuting ? TrueColor : FalseColor;
        isAcceptingInputImg.color = isAcceptingInput ? TrueColor : FalseColor;
    }

    public void AcceptInput()
    {
        // byte[] temp = ASCIIEncoding.ASCII.GetBytes(userInput.text.ToCharArray(), 0, 1);
        Byte.TryParse(
            userInput.text, System.Globalization.NumberStyles.HexNumber, null, out memory[dataPointer]);
        // memory[dataPointer] = temp.Length > 0 ? temp[0] : (byte)0;
        isAcceptingInput = false;
        userInput.interactable = false;

        UpdateImgUI();
    }

    void DebugStuff()
    {
        // 0-7 = Instructions
        char[] validInsChars = {'<','>','+','-','.',',','[',']'};
        
        // 0 = Newline, 1 = Comment
        char[] validOtherChars = {'\n','#'};

        string s = $"This is a sentence. # This is a comment.\nThis is a sentence on a new line. # This is another comment.\nThis is a sentence on another new line. # This is yet another comment.\n";
        s = "++[>-sdfsdf+.,sfdsf]#This is a comment.\n[ +   ]";
        // ++[>-+.,][+]
        
        Debug.Log(s);

        // Strip comments
        for(int i = 0; i < s.Length; i++)
        {
            if(s[i] == validOtherChars[1])
            {
                for(int j = i; j < s.Length; j++)
                {
                    if(s[j] == validOtherChars[0])
                    {
                        s = s.Remove(i, (j-i)+1);
                        break;
                    }
                }
            }
        }

        // Stripg invalid chars
        // s = s.Trim(validInsChars);
        for(int i = 0; i < s.Length; i++)
        {
            bool invalidCharFound = true;
            for(int j = 0; j < validInsChars.Length; j++)
            {
                if(s[i] == validInsChars[j]) invalidCharFound = false;
            }
            if(invalidCharFound) { s = s.Remove(i,1); i--; }
        }

        // Strip newlines
        s = s.Replace(validOtherChars[0].ToString(), "");

        // Strip whitespaces
        s = s.Replace(" ", "");
    }
}
