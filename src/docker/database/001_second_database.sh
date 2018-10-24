#!/bin/sh

echo "CREATE DATABASE IF NOT EXISTS \`testdb\` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci; ;" | "${mysql[@]}"
echo "GRANT ALL ON \`testdb\`.* TO '"$MYSQL_USER"'@'%' ;" | "${mysql[@]}"
echo 'FLUSH PRIVILEGES ;' | "${mysql[@]}"

"${mysql[@]}" < /docker-entrypoint-initdb.d/testdb.sql_