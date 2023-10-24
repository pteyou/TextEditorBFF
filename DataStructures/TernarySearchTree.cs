namespace TextEditorBFF.DataStructures;

public class TernarySearchTree
{
    private Node? _root;
    public TernarySearchTree() 
    {
        _root = null;
    }

    public void InsertSentence(string sentence, decimal score)
    {
        var sentenceArray = SplitSentence(sentence);
        if (sentenceArray.Length == 0)
        {
            return;
        }
        _root = InternalInsert(_root, sentenceArray, 0, score);
    }

    public IEnumerable<Tuple<string[], decimal>> GetSentencesForCompletion(string sentenceStart, int maxSize)
    {
        if (sentenceStart.Length == 0 || maxSize < 1)
        {
            throw new ArgumentNullException("Error in completion inputs");
        }
        Node? root = FindNode(SplitSentence(sentenceStart), _root, 0);
        if(root == null)
        {
            yield break;
        }
        foreach(var sentence in InternalGetSentencesFromNode(root, maxSize, Array.Empty<string>()))
        {
            yield return sentence;
        }
    }

    private string[] SplitSentence(string sentence)
    {
        throw new NotImplementedException();
    }

    private Node? FindNode(string[] sentence, Node? root, int currentIndex)
    {
        if(root == null)
        {
            return null;
        }
        if(string.Compare(sentence[currentIndex], root.Word) < 0)
        {
            return FindNode(sentence, root.LeftChild, currentIndex);
        }
        else if(string.Compare(sentence[currentIndex], root.Word) > 0)
        {
            return FindNode(sentence, root.RightChild, currentIndex);
        }
        else
        {
            if (currentIndex == sentence.Length - 1)
            {
                return root;
            }
            else 
            {
                return FindNode(sentence, root.MiddleChild, currentIndex + 1);
            }
        }
    }

    private IEnumerable<Tuple<string[], decimal>> InternalGetSentencesFromNode(Node root, int maxSize, string[] buffer)
    {
        if (root == null)
        {
            throw new ArgumentNullException("starting Node for completion cannot be null");
        }
        if(root.IsTerminal || maxSize == 1)
        {
            var result = buffer.Append(root.Word);
            yield return Tuple.Create(result.ToArray(), root.MaxScore);
            yield break;
        }

        if(root.LeftChild != null)
        {
            foreach(var sentence in InternalGetSentencesFromNode(root.LeftChild, maxSize, buffer))
            {
                yield return sentence;
            }
        }

        if(root.RightChild != null)
        {
            foreach(var sentence in InternalGetSentencesFromNode(root.RightChild, maxSize, buffer))
            {
                yield return sentence;
            }
        }

        if(root.MiddleChild != null)
        {
            var augmentedBuffer = buffer.Append(root.MiddleChild.Word).ToArray();
            foreach(var sentence in InternalGetSentencesFromNode(root.MiddleChild, maxSize - 1, augmentedBuffer))
            {
                yield return sentence;
            }
        }
    }

    private Node InternalInsert(Node? root, string[] words, int currentIndex, decimal score)
    {
        var currentWord = words[currentIndex];
        if (string.IsNullOrEmpty(currentWord))
        {
            throw new ArgumentNullException("Can not register an empty content");
        }

        root ??= new Node(currentWord);

        if(string.Compare(currentWord, root.Word) < 0)
        {
            root.LeftChild = InternalInsert(root.LeftChild, words, currentIndex, score);
        }
        else if (string.Compare(currentWord, root.Word) > 0)
        {
            root.RightChild = InternalInsert(root.RightChild, words, currentIndex, score);
        }
        else if (currentIndex < words.Length - 1)
        {
            root.MiddleChild = InternalInsert(root.MiddleChild, words, ++currentIndex, score);
        }
        else
        {
            root.MaxScore = Math.Max(root.MaxScore, score);
        }
        return root;
    }

    internal class Node
    {
        public Node? LeftChild { get; set; }
        public Node? RightChild { get; set; }
        public Node? MiddleChild { get; set; }
        public bool IsTerminal 
        {
            get { return LeftChild == null && RightChild == null && MiddleChild == null; }
        }
        public decimal MaxScore { get; set; }
        public string Word { get; set; }

        public Node(string word)
        {
            MaxScore = 0.0m;
            Word = word;
            LeftChild = null;
            RightChild = null;
            MiddleChild = null;
        }

    }
}