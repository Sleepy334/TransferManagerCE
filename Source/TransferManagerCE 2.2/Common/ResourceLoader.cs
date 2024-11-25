﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using System.Reflection;
using System;

namespace TransferManagerCE.Common
{
    class ResourceLoader
    {
        public static UITextureAtlas? CreateTextureAtlas(string atlasName, string[] spriteNames, string assemblyPath)
        {
            try
            {
                int maxSize = 1024;
                Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                Texture2D[] textures = new Texture2D[spriteNames.Length];
                Rect[] regions = new Rect[spriteNames.Length];

                for (int i = 0; i < spriteNames.Length; i++)
                    textures[i] = loadTextureFromAssembly(assemblyPath + spriteNames[i] + ".png");

                regions = texture2D.PackTextures(textures, 2, maxSize);

                UITextureAtlas textureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
                Material material = UnityEngine.Object.Instantiate(UIView.GetAView().defaultAtlas.material);
                material.mainTexture = texture2D;
                textureAtlas.material = material;
                textureAtlas.name = atlasName;

                for (int i = 0; i < spriteNames.Length; i++)
                {
                    UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo
                    {
                        name = spriteNames[i],
                        texture = textures[i],
                        region = regions[i],
                    };

                    textureAtlas.AddSprite(item);
                }

                return textureAtlas;
            }
            catch (Exception e)
            {
                Debug.Log("CreateTextureAtlas", e);
            }
            return null;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++)
            {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked)
                {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = spriteInfo != null ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name)
        {
            UITextureAtlas[] atlases = (UITextureAtlas[])Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas));
            if (atlases != null)
            {
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (atlases[i].name == name)
                        return atlases[i];
                }
            }

            return UIView.GetAView().defaultAtlas;
        }

        public static Texture2D loadTextureFromAssembly(string path)
        {
            Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);

            byte[] array = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(array, 0, array.Length);

            Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            texture2D.LoadImage(array);

            return texture2D;
        }

        public static Texture2D ConvertRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture active = RenderTexture.active;
            Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = active;

            return texture2D;
        }

        public static void ResizeTexture(Texture2D texture, int width, int height)
        {
            RenderTexture active = RenderTexture.active;

            texture.filterMode = FilterMode.Trilinear;
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);
            renderTexture.filterMode = FilterMode.Trilinear;

            RenderTexture.active = renderTexture;
            Graphics.Blit(texture, renderTexture);
            texture.Resize(width, height);
            texture.ReadPixels(new Rect(0, 0, width, width), 0, 0);
            texture.Apply();

            RenderTexture.active = active;
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        public static void CopyTexture(Texture2D texture2D, Texture2D dest)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
            Graphics.Blit(texture2D, renderTexture);

            RenderTexture active = RenderTexture.active;
            RenderTexture.active = renderTexture;
            dest.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
            dest.Apply();
            RenderTexture.active = active;

            RenderTexture.ReleaseTemporary(renderTexture);
        }

        public static UITextureAtlas GetInbuiltAtlas(string name)
        {
            UITextureAtlas[]? atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            if (atlases != null)
            {
                for (int i = 0; i < atlases.Length; i++)
                {
                    if (atlases[i].name == name)
                    {
                        return atlases[i];
                    }
                }
            }

            return UIView.GetAView().defaultAtlas;
        }
    }
}
