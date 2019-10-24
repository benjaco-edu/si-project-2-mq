FROM node:12

RUN wget -O /usr/local/bin/dumb-init https://github.com/Yelp/dumb-init/releases/download/v1.2.1/dumb-init_1.2.1_amd64 && \
chmod 755 /usr/local/bin/dumb-init

# Create app directory
WORKDIR /usr/src/app

# Bundle app source
COPY . .

RUN npm i

ENTRYPOINT ["/usr/local/bin/dumb-init", "--"]

CMD node index.js
