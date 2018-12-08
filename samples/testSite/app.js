const tus = require('tus-node-server');
const express = require('express');
const server = new tus.Server();
server.datastore = new tus.FileStore({
    path: '/files'
});

const app = express();
const uploadApp = express();
uploadApp.all('*', server.handle.bind(server));
app.get('/',function(req,res){
    res.send('upload site by tian');
});
app.use('/uploads', uploadApp);

const host = '0.0.0.0';
const port = 1080;
app.listen(port, host);