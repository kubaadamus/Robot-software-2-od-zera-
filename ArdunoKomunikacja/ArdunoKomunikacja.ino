
String instring; 
void setup() 
{ 
Serial.begin(115200); 
Serial.setTimeout(100); 
} 
void loop() 
{ 
 
instring = Serial.readString();
if(instring=="a")
{
wyslij(String(instring.length()));
}

}
void wyslij(String co) 
{ 
Serial.print(co); 
} 
