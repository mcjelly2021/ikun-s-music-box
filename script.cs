using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using ScriptedEngine;
using ZenithEngine;

class Halo
{
    public double posLeft;
    public double posTop;
    public double posRight;
    public double posBottom;
    public Color4 color;
    public double life;

    public Halo(double posLeft, double posTop, double posRight, double posBottom, Color4 color, double life)
    {
        this.posLeft = posLeft;
        this.posTop = posTop;
        this.posRight = posRight;
        this.posBottom = posBottom;
        this.color = color;
        this.life = life;
    }
}

class Spark
{
    public Vector2d pos;
    public Vector2d vel;
    public Color4 color;
    public double life;
    public double acceleration;

    public Spark(Vector2d pos, Vector2d vel, Color4 color, double life, double acceleration)
    {
        this.pos = pos;
        this.vel = vel;
        this.color = color;
        this.life = life;
        this.acceleration = acceleration;
    }

    public void Flutter()
    {
        if (vel.X > 0)
        {
            vel.X -= acceleration;
        }
        else if (vel.X < 0)
        {
            vel.X += acceleration;
        }
        pos += vel;
    }
}

public class Script
{
    public string Description = "Modified Example Textured by Choomaypie.";
    public string Preview = "preview.png";

    // ������ͼ����
    Texture keyBlack;
    Texture keyBlackPressed;
    Texture keyWhite;
    Texture keyWhitePressed;
    Texture noteBody;
    Texture noteTop;
    Texture noteBottom;
    Texture line;// ��������
    Texture light;// ����
    Texture halo;// ����
    Texture spark;// ��

    LinkedList<Spark> sparks = new LinkedList<Spark>();// ������
    Dictionary<int, Halo> halos = new Dictionary<int, Halo>();// �����ֵ�
    Dictionary<int, Halo> lights = new Dictionary<int, Halo>();// �����ֵ�

    int maxSparks = 15000;// ���𻨴�������

    Random r = new Random();

    public void Load()
    {
        // ������ͼ
        keyBlack = IO.LoadTexture("keyBlack.png");
        keyBlackPressed = IO.LoadTexture("keyBlackPressed.png");
        keyWhite = IO.LoadTexture("keyWhite.png");
        keyWhitePressed = IO.LoadTexture("keyWhitePressed.png");
        noteBody = IO.LoadTexture("note.png");
        noteTop = IO.LoadTexture("noteEdge.png");
        noteBottom = IO.LoadTexture("noteEdge.png");
        line = IO.LoadTexture("line2.png");
        light = IO.LoadTexture("light.png");
        halo = IO.LoadTexture("halo.png");
        spark = IO.LoadTexture("spark.png");
    }

    public void Render(IEnumerable<Note> notes, RenderOptions options)
    {
        double keyboardHeight = 0.15;
        // �����ټ���Χ�����߶�
        keyboardHeight = keyboardHeight / (options.lastKey - options.firstKey) * 128;
        // ������Ļ���߱ȵ����߶�
        keyboardHeight = keyboardHeight / (1920.0 / 1080.0) * options.renderAspectRatio;

        // ���±������ڽ��ַ���λ��Tick��λ��ת������Ļλ��
        // ������Ԥ�ȼ�������ݶ�����ÿ����һ�������Ż�һ��
        double notePosFactor = 1 / options.noteScreenTime * (1 - keyboardHeight);
        double renderCutoff = options.midiTime + options.noteScreenTime;

        // ���̲��֣�ÿ���������������ұ�Ե����Ϊһ��Ԫ����
        var layout = Util.GetKeyboardLayout(options.firstKey, options.lastKey, new KeyboardOptions());
        // ÿ��������ɫ����Ϊ�ݶȣ�ʹ��512����256��
        var keyColors = new Color4[514];
        var keyPressed = new bool[257];

        // ����ÿ������
        foreach (var note in Util.BlackNotesAbove(notes))
        {
            // ��������ڿɼ�����֮�⣬������
            if (note.hasEnded && note.end < options.midiTime) continue;
            if (note.start > renderCutoff) break;

            // ��ȡ������Ե������
            double top = 1 - (renderCutoff - note.end) * notePosFactor;
            double bottom = 1 - (renderCutoff - note.start) * notePosFactor;
            double left = layout.keys[note.key].left;
            double right = layout.keys[note.key].right;

            // ���һ������ȱ�ٽ�β�������Ķ�����������Ϊ��Ļ����
            if (!note.hasEnded) top = 1;

            // ������޴�С��λ��
            double topCapHeight = (right - left) / noteTop.aspectRatio * options.renderAspectRatio;
            double bottomCapHeight = (right - left) / noteBottom.aspectRatio * options.renderAspectRatio;
            double capTop = top - topCapHeight;
            double capBottom = bottom + bottomCapHeight;
            // �������С�ڱ��ޣ�ʹ�����������Ӧ����
            if (capTop < capBottom)
            {
                capTop = (capTop + capBottom) / 2;
                capBottom = capTop;
                top = capTop + topCapHeight;
                bottom = capBottom - bottomCapHeight;
            }

            // һ������ʹ��������ɫ�ֱ�����ߺ��ұ���ɫ
            Color4 leftCol = note.color.left;
            Color4 rightCol = note.color.right;

            // �����������ȣ�ʹÿ�����������ǰ׼����Ǻڼ����ǵȿ��ģ�note.keyΪ�ټ���ţ��������Ҵ�0��ʼ��
            if ((note.key % 12 == 0) || ((note.key - 5) % 12 == 0))
            {
                right -= layout.keys[note.key].right - layout.keys[note.key + 1].left;
            }

            if (((note.key - 2) % 12 == 0) || ((note.key - 7) % 12 == 0) || ((note.key - 9) % 12 == 0))
            {
                left += layout.keys[note.key - 1].right - layout.keys[note.key].left;
                right -= layout.keys[note.key].right - layout.keys[note.key + 1].left;
            }

            if (((note.key - 4) % 12 == 0) || ((note.key - 11) % 12 == 0))
            {
                left += layout.keys[note.key - 1].right - layout.keys[note.key].left;
            }

            // ��������Ƿ�Ӵ��˼���
            if (note.start < options.midiTime)
            {
                // ��Ǽ�Ϊ������
                keyPressed[note.key] = true;

                // ������ԣ��򽫵�ǰ������ɫ��������ɫ���
                // ��������ǰ�͸���ģ���������õ�
                keyColors[note.key * 2] = Util.BlendColors(keyColors[note.key * 2], note.color.left);
                keyColors[note.key * 2 + 1] = Util.BlendColors(keyColors[note.key * 2 + 1], note.color.right);
            }

            // Render the note body and note caps, with gradient colorʹ�ý�����ɫ��Ⱦ�������ɺ���������
            IO.RenderQuad(left, capTop, right, capBottom, leftCol, rightCol, rightCol, leftCol, noteBody);
            IO.RenderQuad(left, top, right, capTop, leftCol, rightCol, rightCol, leftCol, noteTop);
            IO.RenderQuad(left, capBottom, right, bottom, leftCol, rightCol, rightCol, leftCol, noteBottom);
        }

        // �ظ�ִ�������ѭ�����
        foreach (var note in Util.BlackNotesAbove(notes))
        {
            if (note.hasEnded && note.end < options.midiTime || note.start > renderCutoff) continue;

            double top = 1 - (renderCutoff - note.end) * notePosFactor;
            double bottom = 1 - (renderCutoff - note.start) * notePosFactor;
            double left = layout.keys[note.key].left;
            double right = layout.keys[note.key].right;

            if (!note.hasEnded) top = 1;

            double topCapHeight = (right - left) / noteTop.aspectRatio * options.renderAspectRatio;
            double bottomCapHeight = (right - left) / noteBottom.aspectRatio * options.renderAspectRatio;
            double capTop = top - topCapHeight;
            double capBottom = bottom + bottomCapHeight;
            if (capTop < capBottom)
            {
                capTop = (capTop + capBottom) / 2;
                capBottom = capTop;
                top = capTop + topCapHeight;
                bottom = capBottom - bottomCapHeight;
            }

            Color4 leftCol = note.color.left;
            Color4 rightCol = note.color.right;

            if ((note.key % 12 == 0) || ((note.key - 5) % 12 == 0))
            {
                right -= layout.keys[note.key].right - layout.keys[note.key + 1].left;
            }

            if (((note.key - 2) % 12 == 0) || ((note.key - 7) % 12 == 0) || ((note.key - 9) % 12 == 0))
            {
                left += layout.keys[note.key - 1].right - layout.keys[note.key].left;
                right -= layout.keys[note.key].right - layout.keys[note.key + 1].left;
            }

            if (((note.key - 4) % 12 == 0) || ((note.key - 11) % 12 == 0))
            {
                left += layout.keys[note.key - 1].right - layout.keys[note.key].left;
            }

            if (note.start < options.midiTime)
            {
                keyPressed[note.key] = true;

                keyColors[note.key * 2] = Util.BlendColors(keyColors[note.key * 2], note.color.left);
                keyColors[note.key * 2 + 1] = Util.BlendColors(keyColors[note.key * 2 + 1], note.color.right);

                // ���������е�����
                leftCol = Util.BlendColors(leftCol, new Color4(255, 255, 255, 192));
                rightCol = Util.BlendColors(rightCol, new Color4(255, 255, 255, 192));

                // ����һ��ѭ������У����������ڴ������ж����֮�£��ڴ˴���ѭ����������ƶ����������ж����֮�ڣ�Ŀ����Ϊ��ֻ��Ⱦ�����е��������ҿ��Ը�����������δ�����е�����
                IO.RenderQuad(left, capTop, right, capBottom, leftCol, rightCol, rightCol, leftCol, noteBody);
                IO.RenderQuad(left, top, right, capTop, leftCol, rightCol, rightCol, leftCol, noteTop);
                IO.RenderQuad(left, capBottom, right, bottom, leftCol, rightCol, rightCol, leftCol, noteBottom);
            }
        }

        // �����һ���������һ�����Ǻڼ����������Ա���Ⱦһ������İ׼�
        int firstKey = options.firstKey;
        int lastKey = options.lastKey;
        if (layout.blackKey[firstKey]) firstKey--;
        if (layout.blackKey[lastKey - 1]) lastKey++;

        // ���û�Ч��
        for (int i = firstKey; i < lastKey; i++)
        {
            //�涨���ٶ�
            double velocity = 0.001;

            // NextDouble()������ÿ��ȡһ����ΧΪ[0,1)�����˫���ȸ�����
            if (r.NextDouble() < 0.15)
            {
                // �涨�𻨲���λ��
                var pos = new Vector2d(layout.keys[i].left + (layout.keys[i].right - layout.keys[i].left) * r.NextDouble(), keyboardHeight);
                // �涨ˮƽ����ʹ�ֱ����ĳ��ٶ�
                var vel = new Vector2d(velocity * (r.NextDouble() - 0.5) * 4, velocity);
                // �涨��������
                var life = 90.0 + (60 * r.NextDouble());
                // ��������������������
                sparks.AddLast(new Spark(pos, vel, keyColors[i * 2], life, 0.00002));// 0.00002Ϊ���ٶ�
                // ����Ŀ�����涨ֵ��ɾ�������ĵ�һ���ڵ�
                if (sparks.Count > maxSparks)
                {
                    sparks.RemoveFirst();
                }
            }
        }

        for (int i = firstKey; i < lastKey; i++)
        {
            // ֻ��Ⱦ�׼�
            if (!layout.blackKey[i])
            {
                Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                if (keyPressed[i])
                {
                    IO.RenderQuad(layout.keys[i].left, keyboardHeight, layout.keys[i].right, 0, leftCol, leftCol, rightCol, rightCol, keyWhitePressed);
                }
                else
                {
                    IO.RenderQuad(layout.keys[i].left, keyboardHeight, layout.keys[i].right, 0, leftCol, leftCol, rightCol, rightCol, keyWhite);
                }
            }
        }

        for (int i = firstKey; i < lastKey; i++)
        {
            // ֻ��Ⱦ�ڼ�
            if (layout.blackKey[i])
            {
                Color4 leftCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2]);
                Color4 rightCol = Util.BlendColors(new Color4(255, 255, 255, 255), keyColors[i * 2 + 1]);

                if (keyPressed[i])
                {
                    IO.RenderQuad(layout.keys[i].left, keyboardHeight, layout.keys[i].right, keyboardHeight / 3, leftCol, leftCol, rightCol, rightCol, keyBlackPressed);
                }
                else
                {
                    IO.RenderQuad(layout.keys[i].left, keyboardHeight, layout.keys[i].right, keyboardHeight / 3, leftCol, leftCol, rightCol, rightCol, keyBlack);
                }
            }
        }

        // ����������������Ⱦÿ����
        var sparkNode = sparks.First;
        while (sparkNode != null)
        {
            var p = sparkNode.Value;
            // Ʈ��Ч����ؼ���
            p.Flutter();
            // ����˥��
            p.life -= 1;
            // ������С
            double size = 0.001 + r.NextDouble() / 100;

            var opacity = Math.Min(1, p.life / 10 / 4);
            var color = p.color;

            // �ı�͸���ȣ�AΪAlphaͨ����
            color.A *= (float)opacity;

            IO.RenderShape(p.pos + new Vector2d(size, size), p.pos + new Vector2d(-size, size), p.pos - new Vector2d(size, size), p.pos - new Vector2d(-size, size), color, spark); 
            // ��������С����ʱɾ����Ӧ�ڵ�
            var _sparkNode = sparkNode;
            sparkNode = sparkNode.Next;
            if (p.life < 0)
            {
                sparks.Remove(_sparkNode);
            }
        }

        // ��Ⱦ���̶�������
        IO.RenderQuad(layout.keys[firstKey].left, keyboardHeight + ((layout.keys[144].right - layout.keys[144].left) / 1.8), layout.keys[lastKey].right, keyboardHeight - ((layout.keys[144].right - layout.keys[144].left) / 1.8), new Color4(255, 255, 255, 255), line);

        for (int i = firstKey; i < lastKey; i++)
        {
            double left = layout.keys[i].left;
            double right = layout.keys[i].right;
            Color4 lightedCol = Util.BlendColors(keyColors[i * 2], new Color4(255, 255, 255, 192));

            // ������
            if ((i % 12 == 0) || ((i - 5) % 12 == 0))
            {
                right -= layout.keys[i].right - layout.keys[i + 1].left;
            }

            if (((i - 2) % 12 == 0) || ((i - 7) % 12 == 0) || ((i - 9) % 12 == 0))
            {
                left += layout.keys[i - 1].right - layout.keys[i].left;
                right -= layout.keys[i].right - layout.keys[i + 1].left;
            }

            if (((i - 4) % 12 == 0) || ((i - 11) % 12 == 0))
            {
                left += layout.keys[i - 1].right - layout.keys[i].left;
            }

            Halo theHalo;
            Halo theLight;

            // ������ǰ��µ�
            if (keyPressed[i])
            {
                /* ���� */
                // ����ֵ�����û�ж�Ӧ�ļ�
                if (halos.ContainsKey(i))
                {
                    theHalo = halos[i];
                    // ������������
                    if (theHalo.life < 1)
                    {
                        theHalo.life += 0.1;
                    }
                    // �ı�͸����
                    var opacity = Math.Min(1, theHalo.life);
                    lightedCol.A *= (float)opacity;

                    theHalo.color = lightedCol;
                }
                else
                {
                    lightedCol.A *= (float)0.1;
                    theHalo = new Halo(left - ((right - left) * 3), keyboardHeight + ((right - left) * 5), right + ((right - left) * 3), keyboardHeight - ((right - left) * 3), lightedCol, 0.1);
                }

                IO.RenderQuad(theHalo.posLeft, theHalo.posTop, theHalo.posRight, theHalo.posBottom, theHalo.color, theHalo.color, theHalo.color, theHalo.color, halo);
                // ����ֵ����ǲ��ǲ�������Ӧ��
                if (!halos.ContainsKey(i))
                {
                    // �����ټ������Ϊ����������Ϊֵ
                    halos.Add(i, theHalo);
                }
                else
                {
                    // ������������
                    halos[i] = theHalo;
                }

                /* ���� */
                // ���������������ȫһ��
                if (lights.ContainsKey(i))
                {
                    theLight = lights[i];
                    if (theLight.life < 1)
                    {
                        theLight.life += 0.1;
                    }
                    var opacity = Math.Min(1, theLight.life);
                    lightedCol.A *= (float)opacity;
                    theLight.color = lightedCol;
                }
                else
                {
                    theLight = new Halo(left - ((right - left) * 4), keyboardHeight + 0.005 + ((right - left) * 6), right + ((right - left) * 4), keyboardHeight + 0.005, lightedCol, 0.1);
                }
                IO.RenderQuad(theLight.posLeft, theLight.posTop, theLight.posRight, theLight.posBottom, theLight.color, theLight.color, theLight.color, theLight.color, light);
                if (!lights.ContainsKey(i))
                {
                    lights.Add(i, theLight);
                }
                else
                {
                    lights[i] = theLight;
                }
            }
            // �������û�а��µ�
            else
            {
                /* ���� */
                if (halos.ContainsKey(i))
                {
                    theHalo = halos[i];
                    // ��������С��0ʱ���ֵ�ɾ����Ӧ�ļ���ֵ
                    if (theHalo.life < 0)
                    {
                        halos.Remove(i);
                    }
                    else
                    {
                        // ����˥��
                        theHalo.life -= 0.04;
                        // ͸����˥��
                        Color4 haloCol = theHalo.color;
                        var opacity = Math.Min(1, theHalo.life);
                        haloCol.A *= (float)opacity;

                        IO.RenderQuad(theHalo.posLeft, theHalo.posTop, theHalo.posRight, theHalo.posBottom, haloCol, haloCol, haloCol, haloCol, halo);
                    }
                }

                /* ���� */
                // ͬ��
                if (lights.ContainsKey(i))
                {
                    theLight = lights[i];
                    if (theLight.life < 0)
                    {
                        lights.Remove(i);
                    }
                    else
                    {
                        theLight.life -= 0.04;
                        Color4 lightCol = theLight.color;
                        var opacity = Math.Min(1, theLight.life);
                        lightCol.A *= (float)opacity;

                        IO.RenderQuad(theLight.posLeft, theLight.posTop, theLight.posRight, theLight.posBottom, lightCol, lightCol, lightCol, lightCol, light);
                    }
                }
            }
        }
    }

    public void RenderInit(RenderOptions options)
    {

    }

    public void RenderDispose()
    {
        // ��Ⱦ����ʱ��������������ֵ�
        sparks.Clear();
        halos.Clear();
        lights.Clear();
    }
}