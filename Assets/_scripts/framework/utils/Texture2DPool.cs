// 
// Copyright (c) 2022 Off The Beaten Track UG
// All rights reserved.
// 
// Maintainer: Jens Bahr
// 

using OTBT.Framework.Utils;
using System.Collections.Generic;
using UnityEngine;

public class Texture2DPool : Singleton<Texture2DPool>
{
    List<Texture> m_LoadedTextures = new List<Texture>();
    List<Texture2D> m_LoadedTextures2D = new List<Texture2D>();
    List<Sprite> m_LoadedSprites = new List<Sprite>();

    public void AddTexture(Sprite ad)
    {
        m_LoadedSprites.Add(ad);
    }

    public void RemoveTexture(Sprite ad, bool destroy = true)
    {
        if (m_LoadedSprites.Contains(ad))
        {
            m_LoadedSprites.Remove(ad);
            if (destroy) Destroy(ad);
        }
    }
    public void AddTexture(Texture2D ad)
    {
        m_LoadedTextures2D.Add(ad);
    }

    public void RemoveTexture(Texture2D ad, bool destroy = true)
    {
        if (m_LoadedTextures2D.Contains(ad))
        {
            m_LoadedTextures2D.Remove(ad);
            if (destroy) Destroy(ad);
        }
    }
    public void AddTexture(Texture ad)
    {
        m_LoadedTextures.Add(ad);
    }

    public void RemoveTexture(Texture ad, bool destroy = true)
    {
        if (m_LoadedTextures.Contains(ad))
        {
            m_LoadedTextures.Remove(ad);
            if(destroy) Destroy(ad);
        }
    }
}
