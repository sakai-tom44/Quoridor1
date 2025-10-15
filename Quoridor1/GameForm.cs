using System; // ��{�N���X���C�u�������g�p
using System.Drawing; // �`��֘A�i���g�p����GUI�J���p�j
using System.Windows.Forms; // Windows�t�H�[���A�v���P�[�V�����p

namespace Quoridor1
{
    /// <summary>
    /// Quoridor�̃Q�[���p�t�H�[���B
    /// �{�[�h�⃌���_���[���Ǘ����A���̓C�x���g�������B
    /// </summary>
    public partial class GameForm : Form
    {
        private Board mainBoard; // �Q�[���{�[�h�̃C���X�^���X
        private bool manualWait = false; // �蓮����ҋ@�t���O

        public int cellSize { get { return pictureBox1.Width / Board.N; } } // 1�}�X�̃T�C�Y�i�s�N�Z���j

        public EvaluateParam[] evaluateParams = new EvaluateParam[2] { new EvaluateParam(), new EvaluateParam() }; // �]���֐��̃p�����[�^

        /// <summary>
        /// �R���X�g���N�^�B�t�H�[�����������B
        /// </summary>
        public GameForm()
        {
            InitializeComponent(); // �t�H�[���f�U�C�i�Ő������ꂽUI��������
        }

        /// <summary>
        /// �t�H�[���̃��[�h���ɌĂ΂��C�x���g�n���h���B
        /// �Q�[�������Z�b�g���ĊJ�n�B
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            reset(); // �Q�[���̏�Ԃ����Z�b�g

            // �y�d�v�z
            // �߂�ǂ������Ĕ񓯊���Update���񂷂��Ƃɂ����������I�ɂ͐�΂ɏC������ꏊ�B
            // ��������̌����ɂȂ�\��������B
            // ���̌��Ƃ���async/await�ŏ����������Ƃ��������B
            Task.Run(() => // �񓯊��^�X�N�Ŏ��s
            {
                while (true) // �������[�v
                {
                    System.Threading.Thread.Sleep(1); // �ҋ@����CPU���ׂ��y��

                    if (mainBoard.gameOver)
                    {
                        if (Board.autoReset) // �������Z�b�g���L���Ȃ�
                        {
                            System.Threading.Thread.Sleep(500); // 0.5�b�ҋ@
                            reset(); // �Q�[�������Z�b�g
                        }
                        continue; // �Q�[�����I�����Ă���ꍇ�͉������Ȃ�
                    }
                    if (mainBoard.player[mainBoard.currentPlayerNumber].playerType == PlayerType.Manual) // ���݂̃v���C���[���蓮����̏ꍇ
                    {
                        manualWait = true; // �蓮����ҋ@�t���O�𗧂Ă�
                        
                        while (manualWait) // �蓮����ҋ@��
                        {
                            System.Threading.Thread.Sleep(10); // �ҋ@����CPU���ׂ��y��
                        }
                    }
                    else
                    {
                        AI.ComputeNextAction(mainBoard ,mainBoard.currentPlayerNumber); // AI�̎�����s
                    }

                    mainBoard.NextPlayer(); // ��Ԃ����̃v���C���[�ɕύX
                    Renderer.DrawBoard(mainBoard, pictureBox1); // �����Ղ�`��
                    mainBoard.CheckGameOver(); // �Q�[���I�����`�F�b�N
                }
            });
        }

        /// <summary>
        /// ���Z�b�g�{�^�����N���b�N���ꂽ�Ƃ��̃C�x���g�n���h���B
        /// �Q�[�������Z�b�g�B
        /// </summary>
        private void Reset_Button_Click(object sender, EventArgs e)
        {
            reset(); // ���Z�b�g�{�^�����N���b�N���ꂽ�Ƃ��ɃQ�[�������Z�b�g
        }

        /// <summary>
        /// �Q�[�������Z�b�g���A�V����Board��Renderer�𐶐����ĕ`��B
        /// </summary>
        private void reset()
        {
            mainBoard = new Board(evaluateParams); // �V�����Ղ𐶐�����PictureBox�Ɋ֘A�t��

            Renderer.DrawBoard(mainBoard, pictureBox1); // �����Ղ�`��
        }

        /// <summary>
        /// PictureBox��Ń}�E�X�{�^���𗣂������̃C�x���g�n���h���B
        /// �N���b�N�ʒu�ɉ����ĕǂ̐ݒu��ړ������݂�B
        /// </summary>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseShitei(e.X, e.Y); // �}�E�X���W���w�肵�ď��������s
        }

        /// <summary>
        /// �}�E�X�Ŏw�肳�ꂽ���W���������A
        /// �ǂ̐ݒu�܂��̓v���C���[�̈ړ������݂�B
        /// </summary>
        private void mouseShitei(int x, int y)
        {
            if (mainBoard.gameOver) return; // �Q�[�����I�����Ă���ꍇ�͖���
            if (!manualWait) return; // �蓮����ҋ@���łȂ��ꍇ�͖���
            if (mainBoard.player[mainBoard.currentPlayerNumber].playerType != PlayerType.Manual) return; // ���ݎ�Ԃ̃v���C���[���蓮����łȂ��ꍇ�͖���

            bool acted = false; // �s���������������ǂ����̃t���O

            // �c�ǐݒu�̔���i�Z�����E�t�߂�x���W���ǂ����j
            if (x % cellSize < Board.lineWidth || x % cellSize >= (cellSize - Board.lineWidth))
            {
                int xi = (x - 10) / cellSize; // �}�X��x���W���v�Z
                int yi = y / cellSize;       // �}�X��y���W���v�Z
                if (mainBoard.verticalMountable[xi, yi])// �c�ǐݒu�����@���m�F
                {
                    acted = true; // �ǐݒu�����������ꍇ
                    mainBoard.wallManager.PlaceWall(xi, yi, WallOrientation.Vertical); // �c�ǐݒu
                }
            }
            // ���ǐݒu�̔���i�Z�����E�t�߂�y���W���ǂ����j
            else if (y % cellSize < Board.lineWidth || y % cellSize >= (cellSize - Board.lineWidth))
            {
                int xi = x / cellSize;       // �}�X��x���W���v�Z
                int yi = (y - 10) / cellSize; // �}�X��y���W���v�Z
                if (mainBoard.horizontalMountable[xi, yi]) // ���ǐݒu�����@���m�F
                {
                    acted = true; // �ǐݒu�����������ꍇ
                    mainBoard.wallManager.PlaceWall(xi, yi, WallOrientation.Horizontal); // ���ǐݒu
                }
            }
            // �ǂłȂ���΃v���C���[�̈ړ������݂�
            else
            {
                int xi = x / cellSize; // �}�X��x���W���v�Z
                int yi = y / cellSize; // �}�X��y���W���v�Z
                acted = mainBoard.TryMovePlayer(xi, yi); // �v���C���[���ړ�
            }

            // �����s�������������ꍇ�͎�Ԃ��I���
            if (acted)
            {
                manualWait = false; // �蓮����ҋ@�t���O�����낷
            }
        }
    }
}
