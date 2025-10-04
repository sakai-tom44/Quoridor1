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
        private Board board; // �Q�[���{�[�h�̃C���X�^���X

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
        }

        /// <summary>
        /// �Q�[���̐i�s���Ǘ����AAI�v���C���[�̑����S��
        /// </summary>
        /// <remarks> ���݂̃v���C���[�̃^�[�����p���I�ɊĎ����A
        /// <see cref="PlayerType.AI"/> �^�C�v�̃v���C���[�ɑ΂��Ď����I�Ɏ�Ԃ����s�B
        /// ���C���X���b�h���u���b�N���Ȃ��悤�񓯊��Ŏ��s�B</remarks>
        private void gameProgress()
        { 
            bool _loop = true;
            var _ = Task.Run(() => // �񓯊��^�X�N�Ŏ��s
            {
                while (_loop) // �������[�v
                {
                    System.Threading.Thread.Sleep(100); // 100�~���b�ҋ@����CPU���ׂ��y��
                    if (board.player[board.currentPlayer].playerType == PlayerType.AI) // ���݂̃v���C���[��AI�̏ꍇ
                    {
                        board.ai.MakeMove(board.currentPlayer); // AI�̎�����s
                    }
                }
            });
        }

        /// <summary>
        /// �Q�[�������Z�b�g���A�V����Board��Renderer�𐶐����ĕ`��B
        /// </summary>
        private void reset()
        {
            board = new Board(pictureBox1); // �V�����Ղ𐶐�����PictureBox�Ɋ֘A�t��

            board.renderer.DrawBoard(); // �����Ղ�`��

            gameProgress(); // �Q�[���i�s�̃��[�v���J�n
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
            if (board.player[board.currentPlayer].playerType != PlayerType.Manual) return; // ���ݎ�Ԃ̃v���C���[���蓮����łȂ��ꍇ�͖���

            bool acted = false; // �s���������������ǂ����̃t���O

            // �c�ǐݒu�̔���i�Z�����E�t�߂�x���W���ǂ����j
            if (x % board.cellSize < Board.lineWidth || x % board.cellSize >= (board.cellSize - Board.lineWidth))
            {
                int xi = (x - 10) / board.cellSize; // �}�X��x���W���v�Z
                int yi = y / board.cellSize;       // �}�X��y���W���v�Z
                if (board.verticalMountable[xi,yi])// �c�ǐݒu�����@���m�F
                {
                    acted = true; // �ǐݒu�����������ꍇ
                    board.wallManager.SetWall(xi, yi, WallOrientation.Vertical); // �c�ǐݒu
                }
            }
            // ���ǐݒu�̔���i�Z�����E�t�߂�y���W���ǂ����j
            else if (y % board.cellSize < Board.lineWidth || y % board.cellSize >= (board.cellSize - Board.lineWidth))
            {
                int xi = x / board.cellSize;       // �}�X��x���W���v�Z
                int yi = (y - 10) / board.cellSize; // �}�X��y���W���v�Z
                if (board.horizontalMountable[xi, yi]) // ���ǐݒu�����@���m�F
                {
                    acted = true; // �ǐݒu�����������ꍇ
                    board.wallManager.SetWall(xi, yi, WallOrientation.Horizontal); // ���ǐݒu
                }
            }
            // �ǂłȂ���΃v���C���[�̈ړ������݂�
            else
            {
                int xi = x / board.cellSize; // �}�X��x���W���v�Z
                int yi = y / board.cellSize; // �}�X��y���W���v�Z
                acted = board.TryMovePlayer(xi, yi); // �v���C���[���ړ�
            }

            // �����s�������������ꍇ�͔Ֆʂ��ĕ`��
            if (acted)
            {
                board.NextPlayer(); // ��Ԃ����̃v���C���[�ɕύX
            }
        }
    }
}
