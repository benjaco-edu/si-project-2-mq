import pika
import sys
import datetime
import json
import random
import xml.etree.ElementTree as ET

connection = pika.BlockingConnection(pika.ConnectionParameters(host='localrabbit'))
channel = connection.channel()

log = 'PYTHON BROKER'

# creating a random queue 
result = channel.queue_declare(queue='', exclusive=True)
queue_name = result.method.queue


print(' [*] Waiting for incoming stock request. To exit press CTRL+C')

# logger added
channel.basic_publish( exchange='logger_ex', routing_key='', body=log)

# open the broker with keyword ClassA or ClassB
severities = sys.argv[1:]
if not severities:
    #sys.stderr.write("Usage: %s [ClassA] [ClassB]\n" % sys.argv[0])
    sys.stderr.write("Usage: %s [elitebroker] [middelbroker] [uselessbroker]\n" % sys.argv[0])
    sys.exit(1)

# Binds to the unknown queue to receive data. This queue is filled from exchanger stock_type 
for severity in severities:
    channel.queue_bind(exchange='stock_type', queue=queue_name, routing_key=severity)




def main(id,amount, stock,ch,body):
    stocklist = ['apple','IBM','microsoft','facebook','google','youtube','novo','leo','lundbeck','ferring','genmap','schering']
    stockamount = int(amount)
    outputstock = calculate_stock(stockamount)
    outputstocktype = stocktypes(stock, stocklist)
    message = jsonobject(id, outputstock, stock, outputstocktype, body)
    readymessage = json.dumps(message)
    
    routingkey = outputstocktype
     
    
    print("Received and processed Stock data:",outputstock,"for",stock,"with type:",outputstocktype,"and StockId:",id)
   
    print(readymessage)
    ch.basic_publish(exchange='normalizer', routing_key='', body=readymessage,
        properties=pika.BasicProperties( headers={'resptype':outputstocktype})) #delivery_mode=2,
    
    # print logger message
    ch.basic_publish( exchange='logger_ex', routing_key='', body="DATA RECEIVED:"+log+readymessage)
        
    
def calculate_stock(stock):
    if stock <= 100:
        brokerfee = 0.1
    elif stock > 100 & stock <= 1000:
        brokerfee = 0.15
    else:
        brokerfee = 0.2
    subtotal = stock*brokerfee
    totstock = stock - subtotal   
    ran = random.random() * 50

    resulttotstock = totstock * ran  
    return round(resulttotstock,2)

def stocktypes(types, stocklist):
    stockvalue = ''
    if types in stocklist:
        stockvalue ='json'        
    else:
        stockvalue ='json'        
    return stockvalue

def jsonobject(id,newamount, stock, outputstocktype, body):
    output = ''
    if outputstocktype == 'json':
        jdict = {
        "BrokerId":sys.argv[1],"TotalPrice":newamount,"ClientRequestId":id,
        "OriginalMessage":body.decode()}
        output = jdict
        return output
   

   
def callback(ch, method, properties, body):
    print("print", body)
    ans = json.loads(body)
    rows =[]
    for x,y in ans.items():
        rows.append(y)
        
       
    print("Stock requested at:" ,datetime.datetime.now())
    main(rows[0], rows[1], rows[2],ch,body)

channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)
channel.start_consuming()
