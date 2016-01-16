using pcsx2_rr_editor.src.pcsx2_rr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pcsx2_rr_editor.src
{
    public partial class Form1 : Form
    {
        TAS tas = new TAS();
        int selectFrame = 0;    //選択したフレーム
        int headFreame = 0;     //表示用

        public Form1()
        {
            // 初期化
            InitializeComponent();
            
        }

        //=============================================
        // フレームリストの表示
        //=============================================
        private void viewListBox()
        {
            //ファイル情報
            string text = "";
            text += "ファイル情報" + "\r\n";
            text += "総フレーム数：" + tas.info.FrameMax + "\r\n";
            text += "追記回数    ：" + tas.info.Rerecs + "\r\n";

            textBox4.Text = text;

            // 選択フレームの表示
            textBox3.Text = "" + selectFrame;

            // 表示するフレーム数をきめる
            int start = selectFrame - 1000;
            int end = selectFrame + 1000;
            if(start < 0)
            {
                start = 0;
            }
            if( end > tas.keys.Count)
            {
                end = tas.keys.Count;
            }
            
            // リストの更新
            listBox1.BeginUpdate();
            int topindex = headFreame + listBox1.TopIndex;   //表示位置を保存
            listBox1.Items.Clear();
            tas.sort();
            for (int i = start; i < end; i++)
            {
                listBox1.Items.Add(tas.keys[i]);
            }
            //選択する位置を決める
            int index = 0;
            for ( int i=0;i<listBox1.Items.Count;i++)
            {
                PadData obj = (PadData)listBox1.Items[i];
                if ( obj.frame == selectFrame)
                {
                    index = i;
                }
            }
            listBox1.SelectedIndex = index;         //選択フレームを調整
            listBox1.TopIndex = topindex - start;   //表示位置を調整
            listBox1.EndUpdate();
            headFreame = start; //表示用




        }
        //=============================================
        // 引数のチェックリスト(ボタン)の表示
        //=============================================
        private void viewCheckedListBox(CheckedListBox list,TextBox text)
        {
            text.Text = string.Join(",", tas.keys[selectFrame].key);

            for (int i = 0; i < list.Items.Count; i++)
            {
                bool fbutton = tas.keys[selectFrame].isPushKey(list.Items[i].ToString());
                list.SetItemChecked(i, fbutton );
            }
        }
        //=============================================
        // チェックリストの結果からPadDataを作成
        //=============================================
        private PadData createPadData()
        {
            CheckedListBox list = checkedListBox2;
            PadData pad = new PadData();

            // 現在選択されているframeを代入
            pad.set(tas.keys[selectFrame]);

            // textから
            string[] strs = textBox1.Text.Split(',');
            if(strs.Length > 0)
            {
                for(int i = 0; i < strs.Length; i++)
                {
                    if (i >= pad.key.Length) break;
                    byte n;
                    if (!byte.TryParse(strs[i], out n)) continue;

                    pad.key[i] = n;
                }
            }


            //表示通りに更新
            for (int i = 0; i < list.Items.Count; i++)
            {
                pad.setKey(list.Items[i].ToString(), list.GetItemChecked(i));
            }

            return pad;
        }

        //=============================================
        // 読み込み
        //=============================================
        private void 読み込みToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                //OpenFileDialogクラスのインスタンスを作成
                OpenFileDialog ofd = new OpenFileDialog();
                //[ファイルの種類]に表示される選択肢を指定する
                ofd.Filter = "p2mファイル(*.p2m)|*.p2m|すべてのファイル(*.*)|*.*";
                //ダイアログを表示する
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tas.load(ofd.FileName);
                    viewListBox();
                }

            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        //=============================================
        // 保存
        //=============================================
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // SaveFileDialogクラスのインスタンスを作成
                SaveFileDialog sfd = new SaveFileDialog();
                // 拡張子を自動的に付加する
                sfd.AddExtension = true;
                // [ファイルの種類] ボックスに表示される選択肢を設定する
                sfd.Filter = "p2mファイル(*.p2m)|*.p2m|すべてのファイル(*.*)|*.*";

                //ダイアログを表示する
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    tas.save(sfd.FileName);
                }

                
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        //=============================================
        // フレームのリストをクリック時
        //=============================================
        int oldselectindex = -1;
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (oldselectindex == listBox1.SelectedIndex) return;
            oldselectindex = listBox1.SelectedIndex;
            try
            {
                //選択フレームを更新
                selectFrame = int.Parse( listBox1.Text.Substring(0, listBox1.Text.IndexOf(":")));

                //表示
                viewListBox();
                viewCheckedListBox(checkedListBox1,textBox2);
                
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        //=============================================
        // ボタン関係
        //=============================================
        private void button1_Click(object sender, EventArgs e)
        {
            //編集用をセット
            viewCheckedListBox(checkedListBox2, textBox1);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //更新
            PadData pad = createPadData();
            tas.keys[selectFrame].set(pad);
            
            //表示
            viewListBox();
            viewCheckedListBox(checkedListBox1, textBox2);

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //削除
            for (int i= selectFrame; i < tas.keys.Count;i++)
            {
                tas.keys[i].frame--;
            }
            tas.keys.RemoveAt(selectFrame);

            //表示
            viewListBox();
            viewCheckedListBox(checkedListBox1, textBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //挿入
            PadData pad = createPadData();
            for (int i = selectFrame; i < tas.keys.Count; i++)
            {
                tas.keys[i].frame++;
            }
            tas.keys.Insert(selectFrame, pad);

            //表示
            viewListBox();
            viewCheckedListBox(checkedListBox1, textBox2);
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            //表示フレームがかわったらそこに移動
            int val;
            if (!int.TryParse(textBox3.Text, out val))
            {
                //数字じゃない場合は何もしない
                return;
            }


            // フレームを更新して描画
            selectFrame = val;
            viewListBox();
        }
    }
}
