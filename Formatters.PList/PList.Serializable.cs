// <copyright file="PList.Serializable.cs" company="RossKing">
// Copyright (c) RossKing. All rights reserved.
// </copyright>

namespace Formatters.PList;

/// <content>
/// <see cref="System.Runtime.Serialization.ISerializable"/> implementation.
/// </content>
[Serializable]
public partial class PList : System.Runtime.Serialization.ISerializable
{
    private const string PListBinaryField = "bplist";

    /// <summary>
    /// Initializes a new instance of the <see cref="PList"/> class.
    /// </summary>
    /// <param name="serializationInfo">The serialization information.</param>
    /// <param name="streamingContext">The streaming context.</param>
    protected PList(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
    {
        if (serializationInfo.GetValue(PListBinaryField, typeof(byte[])) is byte[] bytes)
        {
            PList bplist;
            using (var memoryStream = new MemoryStream(bytes!))
            {
                var formatter = new PListBinaryFormatter { Context = streamingContext };
                bplist = (PList)formatter.Deserialize(memoryStream);
            }

            // copy the values
            foreach (var item in bplist)
            {
                this.Add(item);
            }
        }
    }

    /// <inheritdoc/>
    public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
    {
        var formatter = new PListBinaryFormatter { Context = context };
        using var memoryStream = new MemoryStream();
        formatter.Serialize(memoryStream, this);
        info.AddValue(PListBinaryField, memoryStream.ToArray());
    }
}
