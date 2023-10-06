using System; // importa o namespace System, que fornece tipos fundamentais e classes base para todos os aplicativos .NET. Inclui classes comumente usadas como String, Console e Math
using System.Collections.Generic; //contém classes e interfaces para criar e trabalhar com coleções genéricas, como listas, dicionários.
using System.ComponentModel; //fornece tipos e interfaces que oferecem suporte à programação baseada em componentes, incluindo vinculação de dados e eventos.
using System.Data; //fornece classes para acessar e gerenciar dados de várias fontes de dados, como bancos de dados.
using System.Drawing; //fornece classes para criar e manipular gráficos e imagens.
using System.Linq; //fornece classes e métodos de extensão para consultar dados usando Language Integrated Query (LINQ).
using System.Text; //fornece classes para codificar e decodificar conjuntos de caracteres, trabalhar com strings e outras operações relacionadas a texto.
using System.Windows.Forms; //contém classes para criar aplicativos baseados no Windows e controles de interface do usuário.
using System.Threading; //fornece classes e interfaces para gerenciar multi-threading e sincronização.
using System.Drawing.Imaging; //contém classes para trabalhar com formatos de imagem e manipular imagens em nível baixo.
using System.Windows.Forms.DataVisualization.Charting; //contém classes e componentes para criar e personalizar visualizações de dados, incluindo tabelas e gráficos, em aplicativos Windows Forms.
using System.Globalization; //permite vírgulas como casa decimal
using System.Runtime.InteropServices;
//namespace principal
namespace RoboticArm
{
    public partial class Form1 : Form
    {
        //variáveis para conexão entre C# e Coppelia

        public int clientID = -1; //declaração inicial de variável que se conectará ao coppelia
        public int portNb = 19997;  //porta padrão para conexão ao coppelia
        //Juntas
        public int Junta1;
        public int Junta2;
        public int Junta3;
        public int Junta4;
        public int elem_final;


        //Sensores
        public float Vision_sensor_lat = 0;
        public float Vision_sensor_cima = 0;
        public float Vision_sensor_frente = 0;

        //handles
        public int Visionsensor;
        public int VisionsensorC;
        public int VisionsensorF;



        public Form1()
        {


            // método inicializa os componentes no formulário. 
            InitializeComponent();

            //adiciona linhas data grid view
            dataGridView1.Columns.Add("Comando", "Comando"); //identificador, nome na coluna
            dataGridView1.Columns.Add("x", "X");
            dataGridView1.Columns.Add("y", "Y");
            dataGridView1.Columns.Add("z", "Z");
            dataGridView1.Columns.Add("theta", "Theta");

            //manipulador de eventos
            textBox8.KeyDown += textBox8_KeyDown;
            textBox9.KeyDown += textBox9_KeyDown;
            textBox10.KeyDown += textBox10_KeyDown;
            textBox11.KeyDown += textBox11_KeyDown;
            button11.Click += button11_Click;


            //inicia em PTP 
            radioPTP.Checked = true;

            
            textBox9.Enabled = false;
            textBox10.Enabled = false;
            textBox11.Enabled = false;
           



        }



        private void UpdateLabelsAndTrackbars()
        {

            // Cria um array para armazenar as posições das juntas
            float[] jointPositions = new float[4];

            // Obtém as posições das juntas a partir do Coppellia
            Wrapper.simxGetJointPosition(clientID, Junta1, ref jointPositions[0], simx_opmode.oneshot_wait);
            Wrapper.simxGetJointPosition(clientID, Junta2, ref jointPositions[1], simx_opmode.oneshot_wait);
            Wrapper.simxGetJointPosition(clientID, Junta3, ref jointPositions[2], simx_opmode.oneshot_wait);
            Wrapper.simxGetJointPosition(clientID, Junta4, ref jointPositions[3], simx_opmode.oneshot_wait);

            // Atualiza os valores dos trackbars com base nas posições das juntas
            trackBar1.Value = CalculateTrackBarValue1(jointPositions[0]);
            trackBar2.Value = CalculateTrackBarValue2(jointPositions[1]);
            trackBar3.Value = CalculateTrackBarValue3(jointPositions[2]);
            trackBar4.Value = CalculateTrackBarValue4(jointPositions[3]);

            // Atualiza labels com informações das juntas
            label1.Text = (jointPositions[0] * 180f / (float)Math.PI).ToString("F2") + "°";
            label4.Text = (jointPositions[1] * 180f / (float)Math.PI).ToString("F2") + "°";
            label5.Text = jointPositions[2].ToString("F3") + " m";
            label6.Text = (jointPositions[3] * 180f / (float)Math.PI).ToString("F2") + "°";
        }


        //executado quando o formulário é carregado
        private void Form1_Load(object sender, EventArgs e)
        {

            // Inicia o timer
            timer1.Start();
            // Finaliza qualquer conexão existente
            Wrapper.simxFinish(clientID);
            // Inicia nova conexão com Coppelia
            clientID = Wrapper.simxStart("127.0.0.1", portNb, true, true, 500, 5);
            // Verifica se conexão foi bem-sucedida
            if (clientID != -1)
            {
                label3.Text = "CONECTADO AO V-REP";
            }
            else
            {
                label3.Text = "NÃO CONECTADO";
            }
        }


        //indica se a simulação está em execução
        private bool simulationRunning = false;

        private void button1_Click(object sender, EventArgs e)
        {
            if (!simulationRunning)
            {
                // Inicia a simulação se não estiver em execução
                timer1.Enabled = true;
                Wrapper.simxStartSimulation(clientID, simx_opmode.oneshot);
                button1.Text = "STOP";
                simulationRunning = true;
            }
            else
            {
                // Interrompe a simulação se estiver em execução
                Wrapper.simxStopSimulation(clientID, simx_opmode.oneshot);

                button1.Text = "START";
                simulationRunning = false;
                dataGridView1.Rows.Clear();
                radioPTP.Checked = true;
       
            }

        }

        public class RobotPosition
        {
            public double X { get; set; }
            public double Y { get; set; }
            // Classe que representa uma posição com base em x e y 
        }



        private void UpdateChart()
        {
            //define minimo e máximo do gráfico
            chart1.ChartAreas[0].AxisX.Minimum = -0.4;
            chart1.ChartAreas[0].AxisX.Maximum = 0.6;
            chart1.ChartAreas[0].AxisY.Minimum = -0.2;
            chart1.ChartAreas[0].AxisY.Maximum = 0.7;

            //define nivel da transparencia
            Color Transparente = Color.FromArgb(100, Color.Orange);


            chart1.Series[1].Color = Transparente;

           
            int maxDataPoints = 20;


            // limpa pontos existentes
            chart1.Series[0].Points.Clear();

            chart1.Series[1].Points.Clear();
            // Adiciona novos pontos
            foreach (var position in robotPositions)
            {
                chart1.Series[0].Points.AddXY(position.X, position.Y);

                if (chart1.Series[0].Points.Count > maxDataPoints)
                {
                    // Remove pontos anteriores
                    chart1.Series[0].Points.RemoveAt(0);
                }
            }
            
            //faz area de trabalho 
            chart1.ChartAreas[0].AxisY.Interval = 0.2;

            chart1.ChartAreas[0].AxisX.Interval = 0.2;

            chart1.Series[1].Points.AddXY(0.25,0.25,0.6);      
 
            chart1.Series[1]["PixelPointWidth"] = "95"; 

     
        }

        // Lista para armazenar posições
        private List<RobotPosition> robotPositions = new List<RobotPosition>();

        //método que é executado a cada intervalo de tempo do temporizador
        private void timer1_Tick(object sender, EventArgs e)
        {

            if (simulationRunning)
            {
                double xValue, yValue;

                // Verifica se os valores em textBox3 e textBox4 podem ser convertidos em números
                if (double.TryParse(textBox3.Text, out xValue) && double.TryParse(textBox4.Text, out yValue))
                {

                    // Cria uma nova posição do robô com coordenadas X e Y e a adiciona à lista
                    var newPosition = new RobotPosition
                    {
                        X = xValue,
                        Y = yValue,

                    };
                    robotPositions.Add(newPosition);

                }
                UpdateChart();  // Atualiza o gráfico com x e y
            }

            // Atualiza os valores das posições e trackbars
            UpdatePositionValues();
            UpdateLabelsAndTrackbars();


            // Pega handlers
            Wrapper.simxGetObjectHandle(clientID, "Vision_sensor_lat", out Visionsensor, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "Vision_sensor_cima", out VisionsensorC, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "Vision_sensor_frente", out VisionsensorF, simx_opmode.oneshot_wait);



            Wrapper.simxGetObjectHandle(clientID, "Junta_01", out Junta1, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "Junta_02", out Junta2, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "Junta_03", out Junta3, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "Junta_04", out Junta4, simx_opmode.oneshot_wait);
            Wrapper.simxGetObjectHandle(clientID, "EFinal", out elem_final, simx_opmode.oneshot_wait);

            // bytes separados para cada imagem

            System.IntPtr image1;
            System.IntPtr image2;
            System.IntPtr image3;
            int[] resolution = new int[2];



            //Sensores
            if (Wrapper.simxGetConnectionId(clientID) != -1)
            {
                // Lê a imagem dos sensores
                Wrapper.simxGetVisionSensorImage(clientID, Visionsensor, out resolution[0], out image1, '0', simx_opmode.streaming);
                Wrapper.simxGetVisionSensorImage(clientID, VisionsensorC, out resolution[0], out image2, '0', simx_opmode.streaming);
                Wrapper.simxGetVisionSensorImage(clientID, VisionsensorF, out resolution[0], out image3, '0', simx_opmode.streaming);

                // Processa e exibe as imagens nos PictureBoxes
                if (Wrapper.simxGetVisionSensorImage(clientID, Visionsensor, out resolution[0], out image1, '0', simx_opmode.streaming) == 0)
                {
                    var bmp1 = new Bitmap(resolution[0], resolution[1], 3 * resolution[0], PixelFormat.Format24bppRgb, image1);
                    bmp1.RotateFlip(RotateFlipType.Rotate180FlipX);
                   pictureBox1.Image = bmp1;

                }


                if (Wrapper.simxGetVisionSensorImage(clientID, VisionsensorC, out resolution[0], out image2, '0', simx_opmode.streaming) == 0)
                {
                    var bmp2 = new Bitmap(resolution[0], resolution[1], 3 * resolution[0], PixelFormat.Format24bppRgb, image2);
                    bmp2.RotateFlip(RotateFlipType.Rotate180FlipX);
                   pictureBox2.Image = bmp2;
                }

                if (Wrapper.simxGetVisionSensorImage(clientID, VisionsensorF, out resolution[0], out image3, '0', simx_opmode.streaming) == 0)
                {
                    var bmp3 = new Bitmap(resolution[0], resolution[1], 3 * resolution[0], PixelFormat.Format24bppRgb, image3);
                   bmp3.RotateFlip(RotateFlipType.Rotate180FlipX);//
                    pictureBox3.Image = bmp3;
                }

            }

            Wrapper.simxSynchronousTrigger(clientID);
        }



        //TRACKBAR DA JUNTA 1


        private float CalculateTargetPosition1(int trackBarValue)
        {

            float targetPosition = trackBarValue * (float)Math.PI / +180f;
            return targetPosition;
        }



        private int CalculateTrackBarValue1(float jointPosition)
        {
            // Calcula e retorna a posição ou ângulo desejado com base no trackBarValue
           

            int trackBarValue = (int)(jointPosition / +0.018f);
            return trackBarValue;
        }




        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int trackBarValue = trackBar1.Value;


            // Calcula a posição ou ângulo desejado com base no valor da trackbar
            float targetPosition = CalculateTargetPosition1(trackBarValue);

            // Defina a posição de destino da junta usando o valor calculado

            Wrapper.simxSetJointTargetPosition(clientID, Junta1, targetPosition, simx_opmode.oneshot);

        }


        //TRACKBAR JUNTA 2

        private float CalculateTargetPosition2(int trackBarValue)
        {

            float targetPosition = trackBarValue * (float)Math.PI / +180f;
            return targetPosition;
        }



        private int CalculateTrackBarValue2(float jointPosition)
        {

            int trackBarValue = (int)(jointPosition / +0.018f);
            return trackBarValue;
        }



        private void trackBar2_Scroll(object sender, EventArgs e)
        {

            int trackBarValue = trackBar2.Value;

            float targetPosition = CalculateTargetPosition2(trackBarValue);


            Wrapper.simxSetJointTargetPosition(clientID, Junta2, targetPosition, simx_opmode.oneshot);

        }

        //TRACKBAR JUNTA 3
        //calcula posição de destino de acordo com trackbar
        private float CalculateTargetPosition3(int trackBar3Value)
        {
            // Define alcance de z
            float minZ = 0.05f;
            float maxZ = 0.285f;

            // Calcula fator de escala baseado na restrição
            float scalingFactor = (maxZ - minZ) / (trackBar3.Maximum - trackBar3.Minimum);


            trackBar3Value = Math.Max(trackBar3.Value, trackBar3.Minimum);
            trackBar3Value = Math.Min(trackBar3.Value, trackBar3.Maximum);

            // Calcula target position 
            float targetPosition = minZ + trackBar3Value * scalingFactor;

            return targetPosition;
        }

        //calcula trackbar de acordo com posição da junta
        private int CalculateTrackBarValue3(float jointPosition)
        {

            float minZ = 0.05f;
            float maxZ = 0.285f;

            float scalingFactor = (maxZ - minZ) / (trackBar3.Maximum - trackBar3.Minimum);

            int trackBar3Value = (int)((jointPosition - minZ) / scalingFactor);

            trackBar3Value = Math.Max(trackBar3Value, trackBar3.Minimum);
            trackBar3Value = Math.Min(trackBar3Value, trackBar3.Maximum);

            return trackBar3Value;
        }

        //calcula a posição da junta sem definir a posição de destino.
        private float CalculateJointPosition3(int trackBar3Value)
        {

            float minZ = 0.05f;
            float maxZ = 0.285f;


            float scalingFactor = (maxZ - minZ) / (trackBar3.Maximum - trackBar3.Minimum);


            trackBar3Value = Math.Max(trackBar3Value, trackBar3.Minimum);
            trackBar3Value = Math.Min(trackBar3Value, trackBar3.Maximum);


            float jointPosition = minZ + trackBar3Value * scalingFactor;

            return jointPosition;
        }

        //evento acionado quando o usuário interage com o TrackBa
        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            int trackBar3Value = trackBar3.Value;

            float targetPosition = CalculateTargetPosition3(trackBar3Value);

            Wrapper.simxSetJointTargetPosition(clientID, Junta3, targetPosition, simx_opmode.oneshot);
        }

        //TRACKBAR JUNTA 4

        private float CalculateTargetPosition4(int trackBarValue)
        {

            float targetPosition = trackBarValue * (float)Math.PI / +180f;
            return targetPosition;
        }


        private int CalculateTrackBarValue4(float jointPosition)
        {

            int trackBarValue = (int)(jointPosition / +0.018f);
            return trackBarValue;
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {

            int trackBarValue = trackBar4.Value;

            float targetPosition = CalculateTargetPosition4(trackBarValue);

            Wrapper.simxSetJointTargetPosition(clientID, Junta4, targetPosition, simx_opmode.oneshot);

        }


        private void UpdatePositionValues()
        {
            float auxX = 0;
            string pos1 = "pos1";
            Wrapper.simxGetFloatSignal(clientID, pos1, ref auxX, simx_opmode.oneshot_wait);
            textBox3.Text = auxX.ToString("F3");


            float auxY = 0;
            string pos2 = "pos2";
            Wrapper.simxGetFloatSignal(clientID, pos2, ref auxY, simx_opmode.oneshot_wait);
            textBox4.Text = auxY.ToString("F3");

            float auxZ = 0;
            string pos3 = "pos3";
            Wrapper.simxGetFloatSignal(clientID, pos3, ref auxZ, simx_opmode.oneshot_wait);
            textBox5.Text = auxZ.ToString("F3");

            float auxTh = 0;
            string pos4 = "pos4";
            Wrapper.simxGetFloatSignal(clientID, pos4, ref auxTh, simx_opmode.oneshot_wait);
            float thetaDegrees = auxTh * 180f / (float)Math.PI;
            textBox6.Text = thetaDegrees.ToString("F3");
        }



        //BOTAO "<<" JUNTA 1
        private void button2_Click(object sender, EventArgs e)
        {

            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta1, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            // Define o incremento para cada step
            float increment = -0.5f * (float)Math.PI / 180f;

            // Calcula o novo target adicionando o incremento
            float targetPosition = currentJointPosition + increment;

            // Estabelece nova posição para a junta
            Wrapper.simxSetJointTargetPosition(clientID, Junta1, targetPosition, simx_opmode.oneshot);

            UpdatePositionValues();
            UpdateLabelsAndTrackbars();

        }



        //BOTAO ">>" JUNTA 1




        private void button3_Click(object sender, EventArgs e)
        {

            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta1, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            // Define o incremento para cada step
            float increment = +0.5f * (float)Math.PI / 180f;

            // Calcula o novo target adicionando o incremento
            float targetPosition = currentJointPosition + increment;

            // Estabelece nova posição para a junta
            Wrapper.simxSetJointTargetPosition(clientID, Junta1, targetPosition, simx_opmode.oneshot);

            int trackBarValue = CalculateTrackBarValue1(targetPosition);
            trackBar1.Value = trackBarValue;


            UpdatePositionValues();
            UpdateLabelsAndTrackbars();
            // Adiciona uma etapa de simulação*/



        }





        //BOTAO "<<" JUNTA 2 

        private void button5_Click(object sender, EventArgs e)
        {
            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta2, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            // Define o incremento para cada step
            float increment = -0.5f * (float)Math.PI / 180f;

            // Calcula o novo target adicionando o incremento
            float targetPosition = currentJointPosition + increment;

            // Estabelece nova posição para a junta
            Wrapper.simxSetJointTargetPosition(clientID, Junta2, targetPosition, simx_opmode.oneshot);



            UpdatePositionValues();
            UpdateLabelsAndTrackbars();
            Wrapper.simxSynchronousTrigger(clientID);

        }

        //BOTAO ">>" JUNTA 2
        private void button4_Click(object sender, EventArgs e)
        {

            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta2, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            // Define o incremento para cada step
            float increment = +0.5f * (float)Math.PI / 180f;

            // Calcula o novo target adicionando o incremento
            float targetPosition = currentJointPosition + increment;

            // Estabelece nova posição para a junta
            Wrapper.simxSetJointTargetPosition(clientID, Junta2, targetPosition, simx_opmode.oneshot);


            UpdatePositionValues();
            UpdateLabelsAndTrackbars();


            Wrapper.simxSynchronousTrigger(clientID);

        }





        //BOTAO "DOWN" JUNTA 3
        private void button7_Click(object sender, EventArgs e)
        {
            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta3, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            float increment = -0.005f;

            float targetPosition = currentJointPosition + increment;

            if (targetPosition < 0.0f)
            {
                targetPosition = 0.0f;
            }

            Wrapper.simxSetJointTargetPosition(clientID, Junta3, targetPosition, simx_opmode.oneshot);

            UpdatePositionValues();
            UpdateLabelsAndTrackbars();

            Wrapper.simxSynchronousTrigger(clientID);

        }



        //BOTAO "UP" JUNTA 3
        private void button6_Click(object sender, EventArgs e)
        {
            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta3, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            float increment = 0.005f;

            float targetPosition = currentJointPosition + increment;

            if (targetPosition < 0.0f)
            {
                targetPosition = 0.0f;
            }

            Wrapper.simxSetJointTargetPosition(clientID, Junta3, targetPosition, simx_opmode.oneshot);

            UpdatePositionValues();
            UpdateLabelsAndTrackbars();

            Wrapper.simxSynchronousTrigger(clientID);


        }



        //BOTAO "<<" JUNTA 4
        private void button9_Click(object sender, EventArgs e)
        {
            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta4, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            float increment = -0.5f * (float)Math.PI / 180f;

            float targetPosition = currentJointPosition + increment;

            Wrapper.simxSetJointTargetPosition(clientID, Junta4, targetPosition, simx_opmode.oneshot);

            UpdatePositionValues();
            UpdateLabelsAndTrackbars();

            Wrapper.simxSynchronousTrigger(clientID);

        }


        //BOTAO ">>" JUNTA 4
        private void button8_Click(object sender, EventArgs e)
        {
            float[] jointPosition = new float[1];
            Wrapper.simxGetJointPosition(clientID, Junta4, ref jointPosition[0], simx_opmode.oneshot_wait);
            float currentJointPosition = jointPosition[0];

            float increment = 0.5f * (float)Math.PI / 180f;

            float targetPosition = currentJointPosition + increment;

            Wrapper.simxSetJointTargetPosition(clientID, Junta4, targetPosition, simx_opmode.oneshot);

            UpdatePositionValues();
            UpdateLabelsAndTrackbars();

            Wrapper.simxSynchronousTrigger(clientID);


        }


        //BOTAO "HOME"
        private void button10_Click(object sender, EventArgs e)
        {
            Wrapper.simxSetIntegerSignal(clientID, "Home", 1, simx_opmode.oneshot);
            UpdatePositionValues();
            UpdateLabelsAndTrackbars();
        }


        //envia sinal para Coppelia chamar função Roda
        private void button11_Click(object sender, EventArgs e)
        {

            Wrapper.simxSetIntegerSignal(clientID, "Roda_sinal", 1, simx_opmode.oneshot);

            Wrapper.simxSynchronousTrigger(clientID);
        }




        private void textBox8_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) //se enter for pressionado no textbox, passa valor para float e verifica se está dentro dos limites aceitos
            {
                float x;

                

                if (float.TryParse(textBox8.Text, out x) && x >= 0 && x <= 0.5f)
                {


                    Wrapper.simxSetFloatSignal(clientID, "TextBox8Value", x, simx_opmode.oneshot);
                    textBox9.Enabled = true;
                    textBox9.Focus();
                }
                else
                {
                    MessageBox.Show("valor válido para x entre 0 e 0,5.");
                }
            }
        }

        private void textBox9_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                float y;

                if (float.TryParse(textBox9.Text, out y) && y >= 0.25f && y <= 0.6f)
                {

                    Wrapper.simxSetFloatSignal(clientID, "TextBox9Value", y, simx_opmode.oneshot);
                    textBox10.Enabled = true;
                    textBox10.Focus();
                }
                else
                {
                    MessageBox.Show("Entre valor válido para y: entre 0,250 e 0,6.");
                }
            }
        }
        private void textBox10_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                float z;

                if (float.TryParse(textBox10.Text, out z) && z >= 0.05f && z <= 0.285f)//,25
                {

                    Wrapper.simxSetFloatSignal(clientID, "TextBox10Value", z, simx_opmode.oneshot);
                    textBox11.Enabled = true;
                    textBox11.Focus();
                }
                else
                {
                    MessageBox.Show("Entre valor válido para z: entre 0,050 e 0,285");
                }
            }
        }


        private void textBox11_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

                float thetaDegrees;

                if (float.TryParse(textBox11.Text, out thetaDegrees))
                {
                    if (radioPTP.Checked)
                    {

                        dataGridView1.Rows.Add("PTP", textBox8.Text, textBox9.Text, textBox10.Text, thetaDegrees);
                    }
                    else if (radioLIN.Checked)
                    {

                        dataGridView1.Rows.Add("LIN", textBox8.Text, textBox9.Text, textBox10.Text, thetaDegrees);
                    }

                    //após enter no último textbox, verifica se PTP ou LIN está acionado para incluir linha com todos os parâmetros na tabela

                    textBox8.Clear();
                    textBox9.Clear();
                    textBox10.Clear();
                    textBox11.Clear();

                    textBox9.Enabled = false;
                    textBox10.Enabled = false;
                    textBox11.Enabled = false;
           

                    Wrapper.simxSetFloatSignal(clientID, "TextBox11Value", thetaDegrees, simx_opmode.oneshot);
                    textBox8.Focus();

                    Thread.Sleep(100);
                    Wrapper.simxSetIntegerSignal(clientID, "incluir_sinal", 1, simx_opmode.oneshot);

                }
                else
                {
                    MessageBox.Show("Entre valor válido para theta: entre -180 a 180.");
                }
                Wrapper.simxSynchronousTrigger(clientID);



            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            // Se todos os valores forem válidos, adiciona uma nova linha na tabela. Usado para função Teach
            bool inputValido = true;

            float x, y, z, thetaDegrees;

            if (float.TryParse(textBox3.Text, out x) && x >= 0 && x <= 0.5f)
            {
                if (float.TryParse(textBox4.Text, out y) && y >= 0.25f && y <= 0.6f)  
                {
                    if (float.TryParse(textBox5.Text, out z) && z >= 0.05f && z <= 0.285f)
                    {
                        if (float.TryParse(textBox6.Text, out thetaDegrees))
                        {
                            if (radioPTP.Checked)
                            {
                                dataGridView1.Rows.Add("PTP", x, y, z, thetaDegrees);
                            }
                            else if (radioLIN.Checked)
                            {
                                dataGridView1.Rows.Add("LIN", x, y, z, thetaDegrees);
                            }
                         

                            Wrapper.simxSetFloatSignal(clientID, "TextBox8Value", x, simx_opmode.oneshot);
                            Wrapper.simxSetFloatSignal(clientID, "TextBox9Value", y, simx_opmode.oneshot);
                            Wrapper.simxSetFloatSignal(clientID, "TextBox10Value", z, simx_opmode.oneshot);
                            Wrapper.simxSetFloatSignal(clientID, "TextBox11Value", thetaDegrees, simx_opmode.oneshot);
                            Thread.Sleep(100);
                            Wrapper.simxSetIntegerSignal(clientID, "incluir_sinal", 1, simx_opmode.oneshot);
                        }
                        else
                        {
                            inputValido = false;
                        }
                    }
                    else
                    {
                        inputValido = false;
                    }
                }
                else
                {
                    inputValido = false;
                }
            }
            else
            {
                inputValido = false;
            }

            if (!inputValido)
            {
                MessageBox.Show("Fora da área de trabalho");
            }
        }

        private void radioPTP_CheckedChanged(object sender, EventArgs e)
        {
            if (radioPTP.Checked)
            {
                int PTPValue = 1;
                int LINValue = 0;
                int PegaSoltaValue = 0;


                Wrapper.simxSetIntegerSignal(clientID, "PTPValue", PTPValue, simx_opmode.oneshot);
                Wrapper.simxSetIntegerSignal(clientID, "LINValue", LINValue, simx_opmode.oneshot);
                Wrapper.simxSetIntegerSignal(clientID, "PegaSoltaValue", PegaSoltaValue, simx_opmode.oneshot);
            }


            Wrapper.simxSynchronousTrigger(clientID);
        }

        private void radioLIN_CheckedChanged(object sender, EventArgs e)
        {
            if (radioLIN.Checked)
            {
                int PTPValue = 0;
                int LINValue = 1;
                int PegaSoltaValue = 0;

                Wrapper.simxSetIntegerSignal(clientID, "PTPValue", PTPValue, simx_opmode.oneshot);
                Wrapper.simxSetIntegerSignal(clientID, "LINValue", LINValue, simx_opmode.oneshot);
                Wrapper.simxSetIntegerSignal(clientID, "PegaSoltaValue", PegaSoltaValue, simx_opmode.oneshot);


            }
            Wrapper.simxSynchronousTrigger(clientID);
        }


        private void PegaSolta_CheckedChanged_1(object sender, EventArgs e)
        {
            if (PegaSolta.Checked)
            { //se radio button PegaSolta for acionado, desabilita botões e textboxes até ser desacionado

                {
                    int PegaSoltaValue = 1;

                    button11.Enabled = false;  
                    button12.Enabled = false;                    
                    button13.Enabled = false;
                    button15.Enabled = false;

                    Thread.Sleep(100);
                    button11.Enabled = true;  
                    button12.Enabled = true;
                    button13.Enabled = true;
                    button15.Enabled = true;
                    textBox8.ReadOnly = true;
                    textBox9.ReadOnly = true;
                    textBox10.ReadOnly = true;
                    textBox11.ReadOnly = true;
                 

                    Wrapper.simxSetIntegerSignal(clientID, "PegaSoltaValue", PegaSoltaValue, simx_opmode.oneshot);
                    dataGridView1.Rows.Add("PegaSolta", textBox8.Text, textBox9.Text, textBox10.Text,"");

                    Thread.Sleep(100);
                    Wrapper.simxSetIntegerSignal(clientID, "incluir_sinal", 1, simx_opmode.oneshot);
                }
            }
            else
            {

                button12.Enabled = true;
                textBox8.ReadOnly = false;
                textBox9.ReadOnly = false;
                textBox10.ReadOnly = false;
                textBox11.ReadOnly = false;
            }

            Wrapper.simxSynchronousTrigger(clientID);
        }


        //envia sinal para Coppelia chamar sinal de Loop
        private void button13_Click(object sender, EventArgs e)
        {
            Wrapper.simxSetIntegerSignal(clientID, "Loop_sinal", 1, simx_opmode.oneshot);

            Wrapper.simxSynchronousTrigger(clientID);

        }

        private void button14_Click(object sender, EventArgs e)
        {
            string guia = "1. JOG:\n"
                        + "- Mova as barras para alterar a posição das juntas.\n"
                        + "- Pressione os botões '<<' ou '>>' para ajuste fino das juntas. \n\n"

                        + "2. Gráfico:\n"
                        + "- Visualize a trajetória do atuador final no plano cartesiano com a linha azul.\n"
                        + "- A área pintada de laranja claro representa o espaço de trabalho do robô .\n\n"
                    
                         + "3. Botão Single Cycle:\n"
                         + "- Realiza o movimento uma única vez e para.\n\n"

                         + "4. Botão Continuous:\n"
                         + "- Realizar o movimento determinado repetidas vezes.\n\n"

                         + "5. Botão Teach:\n"
                        + "- Guarda na tabela a posição atual de acordo com a posição das juntas que foram ajustadas manualmente, desde que dentro do espaço de trabalho.\n"
                        + "- Pressione Continuous ou Single Cycle para movimentar o robô.\n\n"

                         + "6. Botão Clear:\n"
                        + "- Limpa valores da tabela\n\n"

                         + "7. Botão Home:\n"
                        + "- Retorna ao ponto inicial.\n\n"

                         + "8. Botão Puzzle:\n"
                        + "- Realiza um programa de empilhar blocos na forma de uma série harmônica, em que, para cada bloco, o conjunto de blocos acima dele tem o centro de massa em sua extremidade, ou seja, na posição limite para cair.\n";

            MessageBox.Show(guia, "Como utilizar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //enviar sinal ao Coppelia para apagar tabela
        private void button15_Click(object sender, EventArgs e)
        {
            Wrapper.simxSetIntegerSignal(clientID, "Apaga_sinal", 1, simx_opmode.oneshot);
            dataGridView1.Rows.Clear();
            Wrapper.simxSynchronousTrigger(clientID);
        }
        
        //programa "block stacking puzzle"
        private void button16_Click(object sender, EventArgs e)
        {

        float[] x = new float[25];
        float[] y = new float[25];
        float[] z = new float[25];
        float [] th = new float[25];        
        int[] incluir_sinal = new int[25];
        string comando = "PTP";
        // 
        for (int i = 0; i < 25; i++)
            //para este programa são necessários 25 movimentos pré definidos
        {
        
            if (i == 0 || i == 1 || i == 3 || i == 8 || i == 9 || i == 11 || i == 24)  //por exemplo, na primeira linha da tabela (i=0) x precisa estar em 0.45. Assim como na segunda, na quarta, etc.
            {
                        
                x[i] = 0.45f;
               
            }
            else if (i == 4 || i == 5 || i == 7 || i == 12 || i == 13 || i == 15 || i == 16 || i == 17 || i == 19 || i == 20 || i == 21 || i == 23)
            {                     
                x[i] = 0.05f;
            }
         

            if (i==0 || i==1 ||i==3 ||i==16 ||i==17 ||i==19)
            
            {
                y[i] = 0.3f;
            }
            else if (i == 8 || i == 9 || i == 11)
            {
                y[i] = 0.5f;
            }
            else if (i == 4 || i == 5 || i == 7)
            {
                y[i] = (0.5f + ((0.1f) / 6)) - (0.01f * (0.5f + ((0.1f) / 6))); 
                //calcula proximo y, que será dado pelo y anterior + o comprimento do bloco (0.1) dividido por 2n, onde n é a posição do bloco na pilha (neste caso, 3)
                //a segunda parte é para fornecer uma tolerância de 1% no valor calculado. Sem essa tolerância os blocos caem rapidamente devido às vibrações que alteram a posição dos mesmos.
            }
            else if (i == 12 || i == 13 || i == 15)
            {
                y[i] = (y[4] + ((0.1f) / 4)) - (0.01f * (y[4] + ((0.1f) / 4))); 
            }
            else if (i == 20 || i == 21 || i == 23)
            {
                y[i] = (y[12] + ((0.1f) / 2)) - (0.01f * (y[12] + ((0.1f) / 2))); 
            }
            else if (i == 24)
            {
                y[i] = 0.25f;
            }
            if (i == 0 || i == 3 || i == 4 || i == 7 || i == 8 || i == 11 || i == 12 || i == 15 || i == 16 || i == 19 || i == 20 || i == 22 ||i == 23 || i == 24)
            {
                z[i] = 0.23f;
            }
            else if (i == 1|| i == 9 || i == 17)
            {
                z[i] = 0.06f;
            }
            else if (i == 5)
            {
                z[i] = 0.11f;
            }
            else if (i == 13)
            {
                z[i] = 0.16f;
            }
            else if (i == 21)
            {
                z[i] = 0.21f;
            }

                th[i] = 0.0f;
            
         
            if (i == 2 || i == 6 || i == 10 || i == 14 || i == 18 || i == 22)
            {
                incluir_sinal[i] = 1;
                PegaSolta.Checked = true;
                radioPTP.Checked = false;
                radioLIN.Checked = false;
                            
                x[i] = x[i - 1];
                y[i] = y[i - 1];
                z[i] = z[i - 1];
                
                Thread.Sleep(100);
             

               Wrapper.simxSynchronousTrigger(clientID);
        

            }
                PegaSolta.Checked = false;
                radioLIN.Checked = false;
                radioPTP.Checked = true;
               
                incluir_sinal[i] = 1;                
                Thread.Sleep(100);
                Wrapper.simxSetFloatSignal(clientID, "TextBox8Value", x[i], simx_opmode.oneshot);
                Wrapper.simxSetFloatSignal(clientID, "TextBox9Value", y[i], simx_opmode.oneshot);
                Wrapper.simxSetFloatSignal(clientID, "TextBox10Value", z[i], simx_opmode.oneshot);
                Wrapper.simxSetFloatSignal(clientID, "TextBox11Value", th[i], simx_opmode.oneshot);
                Thread.Sleep(100);
                dataGridView1.Rows.Add(comando, x[i], y[i], z[i], th[i]);
                Thread.Sleep(100);
                Wrapper.simxSetIntegerSignal(clientID,"incluir_sinal", incluir_sinal[i], simx_opmode.oneshot);
                Wrapper.simxSynchronousTrigger(clientID);
             
        }
                  
        }
              
    }
         
}
